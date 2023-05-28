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
    private Random random;
    private int energy;
    private const int EnergyDepletionAmount = 1; // Amount of energy to deplete on each movement

    public Agent(int numCounters, int maxHearingDistance, Vector2f position)
    {
        counters = new int[numCounters];
        this.position = position;
        currentDirection = 0; // 0: North, 1: South, 2: East, 3: West
        destination = 0;
        this.maxHearingDistance = maxHearingDistance;
        rectangle = new RectangleShape(new Vector2f(3, 3));
        rectangle.Position = position;
        rectangle.FillColor = new Color(
            (byte)RandomHelper.Random.Next(256),
            (byte)RandomHelper.Random.Next(256),
            (byte)RandomHelper.Random.Next(256)
        );
        random = new Random();
        energy = 100; // Initial energy value for each cell
    }

    public void TakeStep()
    {
        int bias = random.Next(10); // Random bias factor

        // Determine the movement direction with bias
        if (bias == 0)
        {
            // Go North
            position.Y -= 1;
            currentDirection = 0;
        }
        else if (bias == 1)
        {
            // Go South
            position.Y += 1;
            currentDirection = 1;
        }
        else if (bias >= 2 && bias <= 6)
        {
            // Go East
            position.X += 1;
            currentDirection = 2;
        }
        else
        {
            // Go West
            position.X -= 1;
            currentDirection = 3;
        }

        // Deplete energy on each movement
        energy -= EnergyDepletionAmount;

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
        Console.WriteLine($"Agent {position} shouting: {valueToShout} (Energy: {energy})");
    }

    public void Listen(Vector2f shoutingAgentPosition, int shoutingValue)
    {
        if (shoutingValue < counters[destination])
        {
            counters[destination] = shoutingValue;
            if (shoutingAgentPosition != position)
            {
                currentDirection = GetDirectionFromPosition(shoutingAgentPosition);
            }
        }
    }

    private int GetDirectionFromPosition(Vector2f otherPosition)
    {
        if (otherPosition.Y < position.Y)
        {
            // North
            return 0;
        }
        else if (otherPosition.Y > position.Y)
        {
            // South
            return 1;
        }
        else if (otherPosition.X > position.X)
        {
            // East
            return 2;
        }
        else
        {
            // West
            return 3;
        }
    }

    public void Draw(RenderWindow window)
    {
        rectangle.Position = position;
        rectangle.FillColor = GetColorBasedOnEnergy();
        window.Draw(rectangle);
    }

    private Color GetColorBasedOnEnergy()
    {
    	float energyPercentage = (float)energy / 100f;
    	byte red = (byte)(255 * (1 - energyPercentage));
    	byte green = (byte)(255 * energyPercentage);
    	byte blue = 0;
    	return new Color(red, green, blue);
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
                if (agent.position.X == agent.destination * 100)
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

