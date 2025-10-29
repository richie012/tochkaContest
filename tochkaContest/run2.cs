using System;
using System.Collections.Generic;
using System.Linq;

class run2
{
    static List<string> Solve(List<(string, string)> edges)
    {
        var graph = new Dictionary<string, HashSet<string>>();
        foreach (var (a, b) in edges)
        {
            if (!graph.ContainsKey(a)) graph[a] = new HashSet<string>();
            if (!graph.ContainsKey(b)) graph[b] = new HashSet<string>();
            graph[a].Add(b);
            graph[b].Add(a);
        }

        var gateways = graph.Keys.Where(k => k.All(char.IsUpper)).ToHashSet();
        var virus = "a";
        var result = new List<string>();

        (Dictionary<string, int> dist, Dictionary<string, string> prev) Bfs(string start)
        {
            var dist = new Dictionary<string, int>();
            var prev = new Dictionary<string, string>();
            var q = new Queue<string>();
            if (!graph.ContainsKey(start)) return (dist, prev);
            q.Enqueue(start);
            dist[start] = 0;
            while (q.Count > 0)
            {
                var node = q.Dequeue();
                foreach (var next in graph[node].OrderBy(x => x))
                {
                    if (!dist.ContainsKey(next))
                    {
                        dist[next] = dist[node] + 1;
                        prev[next] = node;
                        q.Enqueue(next);
                    }
                }
            }

            return (dist, prev);
        }

        bool VirusCanReachGateway(string from)
        {
            var (dist, _) = Bfs(from);
            return gateways.Any(g => dist.ContainsKey(g));
        }

        while (VirusCanReachGateway(virus))
        {
            string? chosenEdge = null;

            foreach (var g in gateways.OrderBy(x => x))
            {
                if (graph.ContainsKey(g) && graph[g].Contains(virus))
                {
                    chosenEdge = $"{g}-{virus}";
                    break;
                }
            }

            if (chosenEdge == null)
            {
                chosenEdge = gateways
                    .OrderBy(g => g)
                    .SelectMany(g => graph.ContainsKey(g) ? graph[g].OrderBy(n => n).Select(n => $"{g}-{n}") : Enumerable.Empty<string>())
                    .FirstOrDefault();
            }

            if (chosenEdge == null) break;

            var parts = chosenEdge.Split('-');
            string G = parts[0], N = parts[1];

            if (graph.ContainsKey(G)) graph[G].Remove(N);
            if (graph.ContainsKey(N)) graph[N].Remove(G);
            result.Add(chosenEdge);

            var (distFromVirus, _) = Bfs(virus);
            var reachableGateways = gateways.Where(g => distFromVirus.ContainsKey(g)).ToList();
            if (!reachableGateways.Any())
            {
                break;
            }

            int bestDist = int.MaxValue;
            string? targetGateway = null;
            foreach (var g in reachableGateways.OrderBy(x => x))
            {
                int d = distFromVirus[g];
                if (d < bestDist || (d == bestDist && string.Compare(g, targetGateway, StringComparison.Ordinal) < 0))
                {
                    bestDist = d;
                    targetGateway = g;
                }
            }

            if (targetGateway == null) break;

            var (distToGateway, _) = Bfs(targetGateway);
            if (!distToGateway.ContainsKey(virus))
            {
                continue;
            }

            if (!graph.ContainsKey(virus) || graph[virus].Count == 0)
            {
                break;
            }

            string? nextNode = graph[virus]
                .OrderBy(n => n)
                .FirstOrDefault(n => distToGateway.ContainsKey(n) && distToGateway[n] == distToGateway[virus] - 1);

            if (nextNode == null)
            {
                break;
            }

            virus = nextNode;
        }

        return result;
    }


    static void Main()
    {
        var edges = new List<(string, string)>();
        while (Console.ReadLine() is { } line && line != "")
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }
       
        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}