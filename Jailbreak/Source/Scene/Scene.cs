using System;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Scene;

public abstract class Scene(Jailbreak game)
{
    protected Jailbreak Game => game;

    /// <summary>
    /// Called the frame that the scene is ready to start.
    /// </summary>
    public abstract void Ready();

    /// <summary>
    /// Called every frame, before Render().
    /// </summary>
    /// <param name="deltaTime">Time since the last frame in seconds.</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// Called every frame, after Update().
    /// </summary>
    /// <param name="deltaTime">Time since the last frame in seconds.</param>
    public abstract void Draw(float deltaTime);

}
