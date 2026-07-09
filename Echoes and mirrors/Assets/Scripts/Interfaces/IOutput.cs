using UnityEngine;

public interface IOutput
{
    GameObject gameObject { get; }
    IInput input { get; }
    void RegisterToInput(IInput inputSource);
}