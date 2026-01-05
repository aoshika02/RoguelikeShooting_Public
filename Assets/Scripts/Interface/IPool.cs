public interface IPool
{
    public bool IsGenericUse { get; set; }
    public void OnReuse();
    public void OnRelease();
}
