using SFML.Graphics;
using SFML.System;
using SFML.Window;

static class Program
{    public static List<Agent> agents = new List<Agent>();
    public static List<FoodItem> foodItems = new List<FoodItem>();
    static void Main(string[] args)
    {
        int numAgents = 5;
        int numCounters = 3;
        int maxHearingDistance = 5;
       
        for (int i = 0; i < numAgents; i++)
        {
            Vector2f position = new Vector2f(
                RandomHelper.Random.Next(800),
                RandomHelper.Random.Next(600)
            );
            agents.Add(new Agent(numCounters, maxHearingDistance, position, 3));
        }

        RenderWindow window = new RenderWindow(new VideoMode(800, 600), "SFML Window");
        window.SetFramerateLimit(60);

        while (window.IsOpen)
        {
            window.DispatchEvents();
            window.Clear(Color.White);

            foreach (Agent agent in agents)
            {
                agent.TakeStep();
                agent.Shout();
                agent.Draw(window);
            }

            foreach (FoodItem foodItem in foodItems)
            {
                foodItem.Draw(window);
            }

            HandleBumpingIntoItemsAndDestinationChange(agents, numCounters);
            ListenToShoutsAndUpdateCounters(agents, maxHearingDistance);

            window.Display();
        }
    }

    private static void HandleBumpingIntoItemsAndDestinationChange(List<Agent> agents, int numCounters)
    {
        foreach (Agent agent in agents)
        {
            if (agent.Position.X == agent.Destination * 100)
            {
                agent.ChangeDestination();
            }
            else if ((int)agent.Position.X % 200 == 0)
            {
                agent.BumpIntoItem((int)agent.Position.X / 200 % numCounters);
            }
        }
    }

    private static void ListenToShoutsAndUpdateCounters(List<Agent> agents, int maxHearingDistance)
    {
        foreach (Agent agent in agents)
        {
            foreach (Agent otherAgent in agents)
            {
                if (otherAgent != agent && Math.Abs(otherAgent.Position.X - agent.Position.X) <= maxHearingDistance * 100)
                {
                    agent.Listen(otherAgent.Position, otherAgent.Counters[agent.Destination]);
                }
            }
        }
    }
}

class FoodItem
{
    public Vector2f Position { get; set; }
    public Vector2f Size { get; set; }
    public Color Color { get; set; }

    public FoodItem(Vector2f position, Vector2f size, Color color)
    {
        Position = position;
        Size = size;
        Color = color;
    }

    public void Draw(RenderWindow window)
    {
        RectangleShape rectangle = new RectangleShape(Size);
        rectangle.Position = Position;
        rectangle.FillColor = Color;
        window.Draw(rectangle);
    }
}


class Agent
{
    public int Id { get; }
    public int[] Counters { get; }
    public Vector2f Position { get; set; }
    public int CurrentDirection { get; private set; }
    public int Destination { get; private set; }
    public int MaxHearingDistance { get; }
    public List<Vector2f> Trail { get; }

    private readonly RectangleShape rectangle;
    private readonly Random random;
    private int movementBias;
    private int energy;

    private const int EnergyDepletionAmount = 1;
    private static int nextId = 0;
    private int stepper = 0;

    public Agent(int numCounters, int maxHearingDistance, Vector2f position, int movementBias)
    {
        Id = nextId++;
        this.movementBias = movementBias;
        Counters = new int[numCounters];
        Position = position;
        CurrentDirection = 0;
        Destination = 0;
        MaxHearingDistance = maxHearingDistance;
        rectangle = new RectangleShape(new Vector2f(3, 3));
        rectangle.Position = position;
        rectangle.FillColor = GetRandomColor();
        random = new Random();
        energy = random.Next(1000);
        Trail = new List<Vector2f>();
    }    

    public void TakeStep()
    {
        stepper++;
        int bias = this.movementBias;

        UpdatePositionAndDirection(bias);
        DepleteEnergy();
        AddPositionToTrail();
        UpdateCounters();
    }

    private void UpdatePositionAndDirection(int bias)
    {
        if (random.NextDouble() < 0.3)
        {
            // 30% of the time, use the provided bias.
            switch(bias)
            {
                case 0:
                    MoveNorth();
                    break;
                case 1:
                    MoveSouth();
                    break;
                case 2:
                    MoveEast();
                    break;
                case 3:
                    MoveWest();
                    break;
            }
        }
        else
        {
            // 70% of the time, choose a direction at random.
            switch(random.Next(4))  // Generates a random integer between 0 and 3.
            {
                case 0:
                    MoveNorth();
                    break;
                case 1:
                    MoveSouth();
                    break;
                case 2:
                    MoveEast();
                    break;
                case 3:
                    MoveWest();
                    break;
            }
        }
    }

    private const int GridWidth = 800;
    private const int GridHeight = 600;

    private void MoveNorth()
    {
        Position = new Vector2f(Position.X, (Position.Y - 1 + GridHeight) % GridHeight);
        CurrentDirection = 0;
    }

    private void MoveSouth()
    {
        Position = new Vector2f(Position.X, (Position.Y + 1) % GridHeight);
        CurrentDirection = 1;
    }

    private void MoveEast()
    {
        Position = new Vector2f((Position.X + 1) % GridWidth, Position.Y);
        CurrentDirection = 2;
    }

    private void MoveWest()
    {
        Position = new Vector2f((Position.X - 1 + GridWidth) % GridWidth, Position.Y);
        CurrentDirection = 3;
    }


    private void DepleteEnergy()
    {
        energy -= EnergyDepletionAmount;

        // Check if energy has depleted.
        if (energy <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // Drop food item at current position
        // The food item will be 10x10 in size and blue in color.
        Program.foodItems.Add(new FoodItem(Position, new Vector2f(10, 10), Color.Blue));

        // Remove this agent instance from the list of agents.
        //Program.agents.Remove(this);
    }

    private void AddPositionToTrail()
    {
        Trail.Add(Position);
    }

    private void UpdateCounters()
    {
        for (int i = 0; i < Counters.Length; i++)
        {
            Counters[i]++;
        }
    }

    public void BumpIntoItem(int counterIndex)
    {
        if (counterIndex >= 0 && counterIndex < Counters.Length)
        {
            Counters[counterIndex] = 0;
        }
    }

    public void ChangeDestination()
    {
        CurrentDirection *= -1;
        Destination = (Destination + 1) % Counters.Length;
    }

    public void Shout()
    {            
        if(stepper % 100 == 0)
        {
            int valueToShout = Counters[Destination] + MaxHearingDistance;
            Console.WriteLine($"Agent: id {Id} pos: {Position} shouting: {valueToShout} (Energy: {energy})");
        }
        
    }

    public void Listen(Vector2f shoutingAgentPosition, int shoutingValue)
    {
        if (shoutingValue < Counters[Destination])
        {
            Counters[Destination] = shoutingValue;
            if (shoutingAgentPosition != Position)
            {
                CurrentDirection = GetDirectionFromPosition(shoutingAgentPosition);
            }
        }
    }

    private int GetDirectionFromPosition(Vector2f otherPosition)
    {
        if (otherPosition.Y < Position.Y)
        {
            return 0; // North
        }
        else if (otherPosition.Y > Position.Y)
        {
            return 1; // South
        }
        else if (otherPosition.X > Position.X)
        {
            return 2; // East
        }
        else
        {
            return 3; // West
        }
    }

    public void Draw(RenderWindow window)
    {
        rectangle.Position = Position;
        rectangle.FillColor = GetColorBasedOnEnergy();
        window.Draw(rectangle);

        foreach (Vector2f trailPosition in Trail)
        {
            RectangleShape trailRectangle = new RectangleShape(new Vector2f(1, 1));
            trailRectangle.Position = trailPosition;
            trailRectangle.FillColor = new Color(150, 150, 150);
            window.Draw(trailRectangle);
        }
    }

    private Color GetColorBasedOnEnergy()
    {
        float energyPercentage = (float)energy / 100f;
        byte red = (byte)(255 * (1 - energyPercentage));
        byte green = (byte)(255 * energyPercentage);
        byte blue = 0;
        return new Color(red, green, blue);
    }

    private Color GetRandomColor()
    {
        return new Color(
            (byte)RandomHelper.Random.Next(256),
            (byte)RandomHelper.Random.Next(256),
            (byte)RandomHelper.Random.Next(256)
        );
    }
}

static class RandomHelper
{
    public static Random Random { get; } = new Random();
}
