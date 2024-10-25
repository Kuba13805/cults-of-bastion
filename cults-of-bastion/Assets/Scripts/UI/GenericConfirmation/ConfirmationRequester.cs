using System;

namespace UI.GenericConfirmation
{
    public class ConfirmationRequester
    {
        public static event Action OnRequestConfirmation; 
        
        public static void Request()
        {
            OnRequestConfirmation?.Invoke();
        }
    }
}