using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameframe.Packages.Utility
{
  public class AsyncOperationAwaiter : INotifyCompletion
  {
    private readonly Func<bool> _asyncOperation;
    private Action _continuation;
    
    public AsyncOperationAwaiter(Func<bool> asyncOperation)
    {
      _asyncOperation = asyncOperation;
      _continuation = null;
      Await();
    }
    
    public void GetResult()
    {
    }

    public AsyncOperationAwaiter GetAwaiter() => this;
    
    public bool IsCompleted => _asyncOperation.Invoke();

    public void OnCompleted(Action continuation)
    {
      _continuation = continuation;
    }

    private async void Await()
    {
      while (!IsCompleted)
      {
        await Task.Yield();
      }
      _continuation?.Invoke();
    }
    
  }
}
