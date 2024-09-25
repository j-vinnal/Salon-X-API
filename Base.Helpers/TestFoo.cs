namespace Base.Helpers;

public class TestFoo
{
    public int State { get; set; }

    public int Add(int a)
    {
        State = State + a;
        return State;
    }

    public int Div(int a)
    {
        State = State / a;
        return State;
    }
}