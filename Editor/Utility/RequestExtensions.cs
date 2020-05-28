namespace Gameframe.Packages.Utility
{
  public static class RequestExtensions
  {
    public static AsyncOperationAwaiter GetAwaiter(this UnityEditor.PackageManager.Requests.Request request)
    {
      return new AsyncOperationAwaiter(()=>request.IsCompleted);
    }
  }
}