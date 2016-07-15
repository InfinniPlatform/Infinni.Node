using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using log4net;

namespace Infinni.Node.CommandHandlers
{
    public class CommandTransactionManager<TContext>
    {
        public CommandTransactionManager(ILog log)
        {
            _log = log;
            _stages = new List<StageInfo>();
        }


        private readonly ILog _log;
        private readonly List<StageInfo> _stages;


        public CommandTransactionManager<TContext> Stage(string name, Func<TContext, Task> execute, Func<TContext, Task> rollback = null)
        {
            _stages.Add(new StageInfo(name, execute, rollback));

            return this;
        }


        public async Task Execute(TContext context)
        {
            var rollbackPath = new Stack<StageInfo>();

            foreach (var stage in _stages)
            {
                rollbackPath.Push(stage);

                _log.InfoFormat(Properties.Resources.ExecutingStageIsStarted, stage);

                try
                {
                    await stage.Execute(context);

                    _log.InfoFormat(Properties.Resources.ExecutingStageIsSuccessfullyCompleted, stage);
                }
                catch (Exception error)
                {
                    _log.ErrorFormat(Properties.Resources.ExecutingStageIsCompletedWithErrors, stage, error);

                    var rollbackErrors = await Rollback(context, rollbackPath);

                    throw new AggregateException(Properties.Resources.ExecutingTransactionFailed, new[] { error }.Concat(rollbackErrors));
                }
            }
        }


        public async Task Rollback(TContext context)
        {
            var rollbackErrors = await Rollback(context, Enumerable.Reverse(_stages));

            if (rollbackErrors.Count > 0)
            {
                throw new AggregateException(Properties.Resources.ExecutingRollbackTransactionFailed, rollbackErrors);
            }
        }


        private async Task<List<Exception>> Rollback(TContext context, IEnumerable<StageInfo> rollbackPath)
        {
            var errors = new List<Exception>();

            foreach (var stage in rollbackPath)
            {
                _log.InfoFormat(Properties.Resources.RollbackStageIsStarted, stage);

                try
                {
                    await stage.Rollback(context);

                    _log.InfoFormat(Properties.Resources.RollbackStageIsSuccessfullyCompleted, stage);
                }
                catch (Exception error)
                {
                    _log.ErrorFormat(Properties.Resources.RollbackStageIsCompletedWithErrors, stage, error);

                    errors.Add(error);
                }
            }

            return errors;
        }


        class StageInfo
        {
            public StageInfo(string name, Func<TContext, Task> execute, Func<TContext, Task> rollback)
            {
                _name = name;
                _execute = execute;
                _rollback = rollback;
            }


            private readonly string _name;
            private readonly Func<TContext, Task> _execute;
            private readonly Func<TContext, Task> _rollback;


            public async Task Execute(TContext context)
            {
                if (_execute != null)
                {
                    try
                    {
                        await _execute(context);
                    }
                    catch (Exception error)
                    {
                        throw new InvalidOperationException(string.Format(Properties.Resources.CantExecuteStage, _name), error);
                    }
                }
            }

            public async Task Rollback(TContext context)
            {
                if (_rollback != null)
                {
                    try
                    {
                        await _rollback(context);
                    }
                    catch (Exception error)
                    {
                        throw new InvalidOperationException(string.Format(Properties.Resources.CantRollbackStage, _name), error);
                    }
                }
            }


            public override string ToString()
            {
                return _name;
            }
        }
    }
}