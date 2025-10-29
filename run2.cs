using System;
using System.Collections.Generic;

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
            var (dist, prev) = Bfs(virus);

            var nearestGateway = gateways
                .Where(g => dist.ContainsKey(g))
                .OrderBy(g => dist[g])
                .ThenBy(g => g, StringComparer.Ordinal)
                .FirstOrDefault();

            if (nearestGateway == null)
                break;

            var path = new List<string>();
            var cur = nearestGateway;
            while (cur != virus && prev.ContainsKey(cur))
            {
                path.Add(cur);
                cur = prev[cur];
            }
            path.Add(virus);
            path.Reverse();

            var nextVirusPos = path.Count > 1 ? path[1] : virus;

            var possibleEdges = new List<(string g, string n)>();
            foreach (var g in gateways.OrderBy(x => x))
            {
                if (!graph.ContainsKey(g)) continue;
                foreach (var n in graph[g].OrderBy(x => x))
                {
                    possibleEdges.Add((g, n));
                }
            }

            string? chosenEdge = null;

            foreach (var (g, n) in possibleEdges)
            {
                var removed = false;
                if (graph.ContainsKey(g) && graph[g].Contains(n))
                {
                    graph[g].Remove(n);
                    removed = true;
                }
                if (graph.ContainsKey(n) && graph[n].Contains(g))
                {
                    graph[n].Remove(g);
                }

                if (!VirusCanReachGateway(virus))
                {
                    chosenEdge = $"{g}-{n}";
                }

                if (removed)
                {
                    graph[g].Add(n);
                    graph[n].Add(g);
                }

                if (chosenEdge != null)
                    break;
            }

            if (chosenEdge == null)
            {
                bool found = false;
                foreach (var g in gateways.OrderBy(x => x))
                {
                    if (!graph.ContainsKey(g)) continue;
                    foreach (var n in graph[g].OrderBy(x => x))
                    {
                        if (path.Contains(n))
                        {
                            chosenEdge = $"{g}-{n}";
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }

            if (chosenEdge == null)
                break;

            var parts = chosenEdge.Split('-');
            string G = parts[0], N = parts[1];
            if (graph.ContainsKey(G)) graph[G].Remove(N);
            if (graph.ContainsKey(N)) graph[N].Remove(G);

            result.Add(chosenEdge);

            virus = nextVirusPos;
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

