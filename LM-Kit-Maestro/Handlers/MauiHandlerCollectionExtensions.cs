using LMKitMaestro.Controls;

namespace LMKitMaestro.Handlers
{
    public static class MauiHandlerCollectionExtensions
    {
        public static IMauiHandlersCollection AddCustomHandlers(this IMauiHandlersCollection handlers)
        {
            handlers.AddHandler(typeof(CustomEntry), typeof(CustomEntryHandler));

            return handlers;
        }
    }
}
