using Flowsharp.Activities;
using Flowsharp.Results;

namespace Flowsharp.Handlers
{
    public abstract class ActivityHandler<T> : ActivityHandlerBase<T> where T : IActivity
    {
        protected HaltResult Halt() => new HaltResult();
        protected TriggerEndpointResult TriggerEndpoint(string name) => new TriggerEndpointResult(name);
        protected ScheduleActivityResult ScheduleActivity(IActivity activity) => new ScheduleActivityResult(activity);
        protected ReturnValueResult SetReturnValue(object value) => new ReturnValueResult(value);
        protected FinishWorkflowResult Finish() => new FinishWorkflowResult();
    }
}