using DatafileGenerator.Data;
using DatafileGenerator.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatafileGenerator;

// Example usage:
// var calculator = new AffectedNotablesCalculator();
// var mapping = calculator.GetNotableToSocketMapping();
// // Returns: { "Flaying": 36931, "Backstabbing": 36932, ... }
// Console.WriteLine($"Flaying is affected by socket: {mapping["Flaying"]}");

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
}

public class SkillNode
{
    public string NodeId { get; set; }
    public int? Skill { get; set; }
    public string Name { get; set; }
    public bool IsNotable { get; set; }
    public bool IsJewelSocket { get; set; }
    public int? Group { get; set; }
    public int? Orbit { get; set; }
    public int? OrbitIndex { get; set; }
}

public class SkillTreeData
{
    public Dictionary<int, SkillGroup> Groups { get; set; }
    public Dictionary<string, SkillNode> Nodes { get; set; }
    public SkillTreeConstants Constants { get; set; }
    public double MinX { get; set; }
    public double MinY { get; set; }
}

public class AffectedNotablesCalculator
{
    private const int BaseJewelRadius = 1800;
    private static readonly int[] Orbit16Angles = { 0, 30, 45, 60, 90, 120, 135, 150, 180, 210, 225, 240, 270, 300, 315, 330 };
    private static readonly int[] Orbit40Angles = {
        0, 10, 20, 30, 40, 45, 50, 60, 70, 80, 90, 100, 110, 120, 130, 135, 140, 150, 160, 170, 180, 190, 200, 210, 220, 225,
        230, 240, 250, 260, 270, 280, 290, 300, 310, 315, 320, 330, 340, 350
    };

    private readonly SkillTreeData _skillTree;

    public AffectedNotablesCalculator()
    {
        _skillTree = new SkillTreeData
        {
            Groups = DataManager.FullTreeData.Groups,
            Nodes = DataManager.FullTreeData.PassiveSkills.ToDictionary(
                kvp => kvp.Key,
                kvp => new SkillNode
                {
                    NodeId = kvp.Key,
                    Skill = (int?)kvp.Value.GraphIdentifier,
                    Name = kvp.Value.Name,
                    IsNotable = kvp.Value.IsNotable,
                    IsJewelSocket = kvp.Value.IsJewelSocket,
                    Group = kvp.Value.Group.HasValue ? (int?)kvp.Value.Group.Value : null,
                    Orbit = kvp.Value.Orbit.HasValue ? (int?)kvp.Value.Orbit.Value : null,
                    OrbitIndex = kvp.Value.OrbitIndex.HasValue ? (int?)kvp.Value.OrbitIndex.Value : null
                }),
            Constants = DataManager.FullTreeData.Constants,
            MinX = DataManager.FullTreeData.MinX,
            MinY = DataManager.FullTreeData.MinY
        };
    }

    /// <summary>
    /// Gets a mapping of Notable skill names to the Jewel Socket ID that affects them.
    /// Returns Dictionary&lt;string, int&gt; where key is notable name, value is socket skill ID.
    /// </summary>
    public Dictionary<string, int> GetNotableToSocketMapping()
    {
        var mapping = new Dictionary<string, int>();

        // Get all Basic and Large (aka cluster) jewel sockets
        var jewelSockets = _skillTree.Nodes.Values.Where(n => n.IsJewelSocket && (n.Name == "Basic Jewel Socket" || n.Name == "Large Jewel Socket")).ToList();

        // For each notable, find the closest jewel socket within radius
        foreach (var notable in _skillTree.Nodes.Values.Where(n => n.IsNotable))
        {
            var notablePos = CalculateNodePos(notable);
            SkillNode closestSocket = null;
            double closestDistance = double.MaxValue;

            foreach (var socket in jewelSockets)
            {
                var socketPos = CalculateNodePos(socket);
                var distance = Distance(notablePos, socketPos);

                if (distance < BaseJewelRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSocket = socket;
                }
            }

            if (closestSocket != null && closestSocket.Skill.HasValue)
            {
                mapping[notable.Name] = closestSocket.Skill.Value;
            }
        }

        return mapping;
    }

    /// <summary>
    /// Calculates the position of a node on the skill tree.
    /// </summary>
    private Point CalculateNodePos(SkillNode node)
    {
        if (!node.Group.HasValue || !node.Orbit.HasValue || !node.OrbitIndex.HasValue)
            return new Point(0, 0);

        var targetGroup = _skillTree.Groups[node.Group.Value];
        var targetAngle = OrbitAngleAt(node.Orbit.Value, node.OrbitIndex.Value);

        var targetGroupPos = ToCanvasCoords(targetGroup.X, targetGroup.Y);
        var targetNodePos = ToCanvasCoords(
            targetGroup.X,
            targetGroup.Y - _skillTree.Constants.OrbitRadii[node.Orbit.Value]
        );

        return RotateAroundPoint(targetGroupPos, targetNodePos, targetAngle);
    }

    /// <summary>
    /// Converts tree coordinates to canvas coordinates.
    /// </summary>
    private Point ToCanvasCoords(double x, double y)
    {
        // With offsetX=0, offsetY=0, scaling=1
        return new Point(
            Math.Abs(_skillTree.MinX) + x,
            Math.Abs(_skillTree.MinY) + y
        );
    }

    /// <summary>
    /// Rotates a point around a center point by the given angle (in degrees).
    /// </summary>
    private Point RotateAroundPoint(Point center, Point target, double angle)
    {
        var radians = (Math.PI / 180) * angle;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);

        var nx = cos * (target.X - center.X) + sin * (target.Y - center.Y) + center.X;
        var ny = cos * (target.Y - center.Y) - sin * (target.X - center.X) + center.Y;

        return new Point(nx, ny);
    }

    /// <summary>
    /// Gets the rotation angle for a node at the given orbit and index position.
    /// </summary>
    private double OrbitAngleAt(int orbit, int index)
    {
        int nodesInOrbit = orbit < _skillTree.Constants.SkillsPerOrbit.Length ? _skillTree.Constants.SkillsPerOrbit[orbit] : 16;

        if (nodesInOrbit == 16)
        {
            int clampedIndex = Math.Clamp(index, 1, 16);
            return Orbit16Angles[16 - clampedIndex];
        }
        else if (nodesInOrbit == 40)
        {
            int clampedIndex = Math.Clamp(index, 1, 40);
            return Orbit40Angles[40 - clampedIndex];
        }
        else
        {
            return 360 - (360.0 / nodesInOrbit) * index;
        }
    }

    /// <summary>
    /// Calculates the Euclidean distance between two points.
    /// </summary>
    private double Distance(Point p1, Point p2)
    {
        var dx = p1.X - p2.X;
        var dy = p1.Y - p2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
