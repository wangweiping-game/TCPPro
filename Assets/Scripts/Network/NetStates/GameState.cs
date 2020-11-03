public abstract class GameState
{
	protected string name;

	public string getName()
	{
		return name;
	}

    public abstract void enter();

    public abstract void update();

    public abstract void exit();
}

