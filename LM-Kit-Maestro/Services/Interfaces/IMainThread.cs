﻿namespace LMKitMaestro.Services;


public interface IMainThread
{
    public void BeginInvokeOnMainThread(Action task);
}
