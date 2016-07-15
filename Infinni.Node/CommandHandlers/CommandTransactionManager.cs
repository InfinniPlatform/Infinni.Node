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

                    if (rollbackErrors.Count > 0)
                    {
                        throw new AggregateException(Properties.Resources.ExecutingTransactionFailed, new[] { error }.Concat(rollbackErrors));
                    }

                    _log.Error(Properties.Resources.ExecutingTransactionFailed);

                    throw;
                }
            }
        }


        private async Task<List<Exception>> Rollback(TContext context, Stack<StageInfo> rollbackPath)
        {
            var errors = new List<Exception>();

            if (rollbackPath.Any(i => i.CanRollback))
            {
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


            public bool CanExecute => _execute != null;

            public bool CanRollback => _rollback != null;


            public async Task Execute(TContext context)
            {
                if (CanExecute)
                {
                    await _execute(context);
                }
            }

            public async Task Rollback(TContext context)
            {
                if (CanRollback)
                {
                    await _rollback(context);
                }
            }


            public override string ToString()
            {
                return _name;
            }
        }
    }
}