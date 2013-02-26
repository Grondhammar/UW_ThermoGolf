using System;

public class Timer 
{
    private DateTime startTime;
    private DateTime stopTime;
    private bool running = false;

    public void Start() 
    {
		this.startTime = DateTime.Now;
		this.running = true;
    }

	public void Stop()
    {
        this.stopTime = DateTime.Now;
        this.running = false;
    }

	// elaspsed time in milliseconds
    public double GetElapsedTime() 
    {
        TimeSpan interval;   

        if (running)
            interval = DateTime.Now - startTime;
        else
            interval = stopTime - startTime;
            
        return interval.TotalMilliseconds;
    }

    // elaspsed time in seconds
    public double GetElapsedTimeSecs() 
    {
        TimeSpan interval;

        if (running)
            interval = DateTime.Now - startTime;
        else
            interval = stopTime - startTime;

        return interval.TotalSeconds;
    }
	
	public void Clear()
	{
		this.stopTime = DateTime.Now;
		this.startTime = DateTime.Now;
	}
	
	public bool GetIsRunning()
	{
		return running;	
	}
}
