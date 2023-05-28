using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

class Agent
{
    public int[] counters;
    public Vector2f position;
    public int currentDirection;
    public int destination;
    public int maxHearingDistance;
    private RectangleShape rectangle;

    public Agent(int numCounters, int maxHearingDistance, Vector2f position)
    {
        counters = new int[numCounters];
        this.position = position;
        currentDirection = 1;
        destination = 0;
        this.maxHearingDistance = maxHearingDistance;

        // Initialize the rectangle shape for rendering
        rectangle = new RectangleShape(new Vector2f(3, 3));
        rectangle.Position = position;
        rectangle.FillColor = new Color(
            (byte)RandomHelper.Random.Next(256),
            (byte)RandomHelper.Random.Next(256),
            (byte)RandomHelper.Random.Next(256)
        );
    }

    public void TakeStep()
    {
        position.X += currentDirection;
        for (int i = 0; i < counters.Length; i++)
        {
            counters[i]++;
        }
    }

    public void BumpIntoItem(int counterIndex)
    {
        counters[counterIndex] = 0;
    }

    public void ChangeDestination()
    {
        currentDirection *= -1;
        destination = (destination + 1) % counters.Length;
    }

    public void Shout()
    {
        int valueToShout = counters[destination] + maxHearingDistance;
        Console.WriteLine($"Agent {position} shouting: {valueToShout}");
    }

    public void Listen(Vector2f shoutingAgentPosition, int shoutingValue)
    {
        if (shoutingValue < counters[destination])
        {
            counters[destination] = shoutingValue;
            if (shoutingAgentPosition != position)
            {
                currentDirection = shoutingAgentPosition.X < position.X ? -1 : 1;
            }
        }
    }

    public void Draw(RenderWindow window)
    {
        window.Draw(rectangle);
    }
}

static class RandomHelper
{
    public static Random Random { get; } = new Random();
}

class Program
{
    static void Main(string[] args)
    {
        // Define the number of agents and their parameters
        int numAgents = 5;
        int numCounters = 3;
        int maxHearingDistance = 5;

        // Create the agents with random positions
        Agent[] agents = new Agent[numAgents];
        for (int i = 0; i < numAgents; i++)
        {
            Vector2f position = new Vector2f(
                RandomHelper.Random.Next(800),
                RandomHelper.Random.Next(600)
            );
            agents[i] = new Agent(numCounters, maxHearingDistance, position);
        }

        // Set up SFML window
        RenderWindow window = new RenderWindow(new VideoMode(800, 600), "SFML Window");
        window.SetFramerateLimit(60);

        // Main loop
        while (window.IsOpen)
        {
            // Handle window events
            window.DispatchEvents();

            // Clear the window
            window.Clear(Color.White);

            // Simulate the behavior of the agents
            foreach (Agent agent in agents)
            {
                agent.TakeStep();
                agent.Shout();
                agent.Draw(window);
            }

            // Handle agents bumping into items or changing destinations
            foreach (Agent agent in agents)
            {
                if ((int)agent.position.X == agent.destination * 100)
                {
                    agent.ChangeDestination();
                }
                else if ((int)agent.position.X % 200 == 0)
                {
                    agent.BumpIntoItem((int)agent.position.X / 200 % numCounters);
                }
            }

            // Listen to shouts and update counters accordingly
            foreach (Agent agent in agents)
            {
                foreach (Agent otherAgent in agents)
                {
                    if (otherAgent != agent && Math.Abs(otherAgent.position.X - agent.position.X) <= maxHearingDistance * 100)
                    {
                        agent.Listen(otherAgent.position, otherAgent.counters[agent.destination]);
                    }
                }
            }

            // Display the window
            window.Display();
        }
    }
}

