namespace Jailbreak.Content.Handler;

public interface IContentHandler<T> : IBaseContentHandler {
    
    T Handle(byte[] data);

}

public interface IBaseContentHandler {}