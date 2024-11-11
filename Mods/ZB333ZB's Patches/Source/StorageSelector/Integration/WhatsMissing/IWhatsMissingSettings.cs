namespace StorageSelector.Integration.WhatsMissing
{
    public interface IWhatsMissingSettings
    {
        bool HideZeroCountIngredients { get; }
        int MaxTooltipsWidth { get; }
    }
}
