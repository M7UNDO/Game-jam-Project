using System;

public interface IInput
{
    event Action<IInput> onTriggered;
    event Action<IInput> onUntriggered;
    bool IsTriggered { get; }
}