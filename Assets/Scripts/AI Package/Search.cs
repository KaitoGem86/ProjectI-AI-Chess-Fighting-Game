using System.Collections.Generic;
using Chess;
using UnityEngine;

namespace AIPackage
{
    public class Search<T> where T : Action
    {
        const int transpositionTableSize = 64000; //kich thuoc cua bang transposition
        const int immediateWinGameScore = 100000;
        const int positiveInfinity = 9999999;

        const int negativeInfinity = -positiveInfinity;

        public event System.Action<T> onSearchComplete;
        public SearchDiagnostics searchDiagnostics;
        TranspositionTable<T> tt; // bảng transposition để tăng toc tim kiem khi xuat hien trang thai da tung duoc kiem tra
        Enviroment<T> env; // trang thai cua tro choi
        protected ActionGenerator<T> actionGenerator;
        protected ActionOrdering<T> actionOrdering;
        protected Evaluation<T> evaluation;

        T bestActionThisIteration; // hanh dong tot nhat trong vong lap tim kiem hien tai
        int bestEvalThisIteration; // diem so cua hanh dong tot nhat trong vong lap tim kiem hien tai
        T bestAction; // hanh dong tot nhat
        int bestEval; // diem so cua hanh dong tot nhat
        T invalidAction; // hanh dong khong hop le 

        bool abortSearch; // dung tim kiem
        AISettings settings; // cai dat cua AI

        int numTranspositions;
        int numQNodes;
        int numNodes;
        int numCutOffs;

        public Search(Enviroment<T> env, AISettings settings, T invalidAction)
        {
            this.env = env;
            this.settings = settings;
            SetEvaluation();
            SetActionGenerator();
            tt = new TranspositionTable<T>(env, transpositionTableSize);
            SetActionordering(actionGenerator, tt, invalidAction);
            this.invalidAction = invalidAction;
        }

        protected virtual void SetEvaluation() { }
        protected virtual void SetActionGenerator() { }
        protected virtual void SetActionordering(ActionGenerator<T> actionGenerator, TranspositionTable<T> tt, T invalidAction) { }

        public void StartSearch()
        {
            //Initialize search settings
            bestEvalThisIteration = bestEval = 0;
            bestActionThisIteration = bestAction = invalidAction;
            tt.enabled = settings.useTranspositionTable;

            if (settings.clearTTEachMove)
            {
                tt.Clear();
            }

            abortSearch = false;
            searchDiagnostics = new SearchDiagnostics();

            if (settings.useIterativeDeepening)
            {
                int targetDepth = (settings.useFixedDepthSearch) ? settings.depth : int.MaxValue;
                for (int searchDepth = 1; searchDepth <= targetDepth; searchDepth++) // tim kiem sau dan
                {
                    SearchActions(searchDepth, 0, negativeInfinity, positiveInfinity);
                    if (abortSearch)
                    {
                        break;
                    }
                    else
                    {
                        bestAction = bestActionThisIteration;
                        bestEval = bestEvalThisIteration;

                        searchDiagnostics.lastCompletedDepth = searchDepth;
                        searchDiagnostics.action = bestAction.ToString();
                        searchDiagnostics.eval = bestEval;

                        if (IsWinGameScore(bestEval) && !settings.endlessSearchMode)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                SearchActions(settings.depth, 0, negativeInfinity, positiveInfinity);
                bestAction = bestActionThisIteration;
                bestEval = bestEvalThisIteration;
            }
            onSearchComplete?.Invoke(bestAction);
        }

        public (T move, int eval) GetSearchResult()
        {
            return (bestAction, bestEval);
        }

        public void EndSearch()
        {
            abortSearch = true;
        }

        private int SearchActions(int depth, int plyFromRoot, int alpha, int beta)
        {
            if (abortSearch)
            {
                return 0;
            }

            Debug.Log("SearchActions");

            if (plyFromRoot > 0)
            {

                alpha = System.Math.Max(alpha, -immediateWinGameScore + plyFromRoot);
                beta = System.Math.Min(beta, immediateWinGameScore - plyFromRoot);
                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            int ttVal = tt.LookupEvaluation(depth, plyFromRoot, alpha, beta);
            if (ttVal != TranspositionTable<T>.lookupFailed)
            {
                numTranspositions++;
                if (plyFromRoot == 0)
                {
                    bestActionThisIteration = tt.GetStoredAction();
                    bestEvalThisIteration = tt.entries[tt.Index].value;
                }
                return ttVal;
            }

            if (depth == 0)
            {
                int evaluation = QuiescenceSearch(alpha, beta);
                return evaluation;
            }

            List<T> actions = actionGenerator.GenerateAllActions(env);
            Debug.Log("Actions: " + actions.Count + " - Depth " + depth );
            actionOrdering.OrderActions(env, actions, settings.useTranspositionTable);

            if (actions.Count == 0)
            {
                if (actionGenerator.IsInCheck())
                {
                    return -immediateWinGameScore + plyFromRoot;
                }
                else
                {
                    return 0;
                }
            }

            int evalType = TranspositionTable<T>.UpperBound;
            T bestActionInThisPosition = invalidAction;
            for (int i = 0; i < actions.Count; i++)
            {
                env.MakeAction(actions[i], true);
                int eval = -SearchActions(depth - 1, plyFromRoot + 1, -beta, -alpha);
                env.UnmakeAction(actions[i], true);
                numNodes++;

                if (eval >= beta)
                {
                    tt.StoreEvaluation(depth, plyFromRoot, beta, TranspositionTable<T>.LowerBound, actions[i]);
                    numCutOffs++;
                    return beta;
                }

                if (eval > alpha)
                {
                    evalType = TranspositionTable<T>.Exact;
                    alpha = eval;
                    bestActionInThisPosition = actions[i];
                    if (plyFromRoot == 0)
                    {
                        bestActionThisIteration = actions[i];
                        bestEvalThisIteration = eval;
                    }
                }
            }
            tt.StoreEvaluation(depth, plyFromRoot, alpha, evalType, bestActionInThisPosition);
            return 0;
        }

        int QuiescenceSearch(int alpha, int beta)
        {
            int eval = evaluation.Evaluate(env);
            searchDiagnostics.numPositionsEvaluated++;
            if (eval >= beta)
            {
                return beta;
            }
            if (eval > alpha)
            {
                alpha = eval;
            }
            var actions = actionGenerator.GenerateAllActions(env, false);
            actionOrdering.OrderActions(env, actions, false);
            for (int i = 0; i < actions.Count; i++)
            {
                env.MakeAction(actions[i], true);
                eval = -QuiescenceSearch(-beta, -alpha);
                env.UnmakeAction(actions[i], true);
                numQNodes++;
                if (eval >= beta)
                {
                    numCutOffs++;
                    return beta;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }
            return alpha;
        }

        public static bool IsWinGameScore(int score)
        {
            const int maxMateDepth = 1000;
            return System.Math.Abs(score) > immediateWinGameScore - maxMateDepth;
        }

        [System.Serializable]
        public class SearchDiagnostics
        {
            public int lastCompletedDepth;
            public bool isBook;
            public string actionVal;
            public string action;
            public int eval;
            public int numPositionsEvaluated;
        }
    }
}