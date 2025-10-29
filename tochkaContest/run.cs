// using System;
// using System.Collections.Generic;
//
// namespace tochkaContest;
//
// class run
// {
//     private static readonly Dictionary<char, int> Energy = new()
//     {
//         ['A'] = 1,
//         ['B'] = 10,
//         ['C'] = 100,
//         ['D'] = 1000
//     };
//     
//
//     private static readonly int[] RoomPositions = [2, 4, 6, 8];
//
//     public readonly struct State(string data, int depth)
//     {
//         public readonly string Data = data;
//         public readonly int Depth = depth;
//
//         public ReadOnlySpan<char> Corridor => Data.AsSpan(0, 11);
//
//         public ReadOnlySpan<char> Room(int r) => Data.AsSpan(11 + r * Depth, Depth);
//
//         public char GetCorridor(int i) => Data[i];
//         
//         private State Move(int fromIdx, int toIdx, char amph)
//         {
//             var arr = Data.ToCharArray();
//             arr[fromIdx] = '.';
//             arr[toIdx] = amph;
//             return new State(new string(arr), Depth);
//         }
//
//         public State MoveRoomToCorridor(int room, int roomIdx, int corridorPos)
//         {
//             var roomGlobalIdx = 11 + room * Depth + roomIdx;
//             return Move(roomGlobalIdx, corridorPos, Data[roomGlobalIdx]);
//         }
//
//         public State MoveCorridorToRoom(int corridorPos, int room, int roomIdx)
//         {
//             var roomGlobalIdx = 11 + room * Depth + roomIdx;
//             return Move(corridorPos, roomGlobalIdx, Data[corridorPos]);
//         }
//     }
//
//     static bool IsFinal(State s)
//     {
//         var corridor = s.Corridor;
//         
//         foreach (var t in corridor)
//             if (t != '.') return false;
//
//         for (int r = 0; r < 4; r++)
//         {
//             var room = s.Room(r);
//             var target = (char)('A' + r);
//             foreach (var t in room)
//                 if (t != target) return false;
//         }
//
//         return true;
//     }
//
//     static IEnumerable<(State next, int cost)> GetMoves(State s)
//     {
//         var depth = s.Depth;
//
//         for (var r = 0; r < 4; r++)
//         {
//             var room = s.Room(r);
//             char target = (char)('A' + r);
//
//             bool roomCorrect = true;
//             for (var i = 0; i < depth; i++)
//             {
//                 var ch = room[i];
//                 if (ch != '.' && ch != target)
//                 {
//                     roomCorrect = false;
//                     break;
//                 }
//             }
//             if (roomCorrect) continue;
//
//             var idx = 0;
//             while (idx < depth && room[idx] == '.') idx++;
//             if (idx == depth) continue;
//
//             var amphType = room[idx];
//             var roomPos = RoomPositions[r];
//
//             for (var pos = 0; pos < 11; pos++)
//             {
//                 if (Array.Exists(RoomPositions, x => x == pos)) continue;
//                 if (!PathClear(s.Corridor, roomPos, pos)) continue;
//
//                 var next = s.MoveRoomToCorridor(r, idx, pos);
//                 var steps = idx + 1 + Math.Abs(roomPos - pos);
//                 var cost = steps * Energy[amphType];
//                 yield return (next, cost);
//             }
//         }
//
//         for (var pos = 0; pos < 11; pos++)
//         {
//             var amph = s.GetCorridor(pos);
//             if (amph == '.' || !Energy.ContainsKey(amph)) continue;
//
//             var targetRoom = amph - 'A';
//             var room = s.Room(targetRoom);
//
//             var hasWrong = false;
//             for (var i = 0; i < room.Length; i++)
//             {
//                 var ch = room[i];
//                 if (ch == '.' || ch == amph) continue;
//                 hasWrong = true;
//                 break;
//             }
//             if (hasWrong) continue;
//
//             if (!PathClear(s.Corridor, pos, RoomPositions[targetRoom])) continue;
//
//             var idx = room.Length - 1;
//             while (idx >= 0 && room[idx] != '.') idx--;
//             if (idx < 0) continue;
//
//             var next = s.MoveCorridorToRoom(pos, targetRoom, idx);
//             var steps = idx + 1 + Math.Abs(pos - RoomPositions[targetRoom]);
//             var cost = steps * Energy[amph];
//             yield return (next, cost);
//         }
//     }
//
//     private static bool PathClear(ReadOnlySpan<char> corridor, int from, int to)
//     {
//         var step = Math.Sign(to - from);
//         for (var i = from + step; i != to + step; i += step)
//             if (corridor[i] != '.') return false;
//         return true;
//     }
//
//     private static int Solve(List<string>? lines)
//     {
//         if (lines == null || lines.Count == 0) return 0;
//
//         var depth = lines.Count - 3;
//         if (depth <= 0) return 0;
//
//         var corridor = (lines[1].Length >= 12) ? lines[1].Substring(1, 11) : "...........";
//
//         var roomsBlock = new char[4 * depth];
//         for (var r = 0; r < 4; r++)
//         {
//             for (var d = 0; d < depth; d++)
//             {
//                 var lineIdx = 2 + d;
//                 var line = lines[lineIdx];
//                 var col = RoomPositions[r] + 1;
//                 var ch = (col < line.Length) ? line[col] : '.';
//                 if (ch != 'A' && ch != 'B' && ch != 'C' && ch != 'D' && ch != '.') ch = '.';
//                 roomsBlock[r * depth + d] = ch;
//             }
//         }
//
//         var initialData = corridor + new string(roomsBlock);
//         var start = new State(initialData, depth);
//
//         var pq = new PriorityQueue<State, int>();
//         var dist = new Dictionary<string, int> { [start.Data] = 0 };
//         pq.Enqueue(start, 0);
//
//         while (pq.TryDequeue(out var state, out var cost))
//         {
//             if (cost > dist[state.Data]) continue;
//             if (IsFinal(state)) return cost;
//
//             foreach (var (next, moveCost) in GetMoves(state))
//             {
//                 var newCost = cost + moveCost;
//                 if (dist.TryGetValue(next.Data, out var best) && newCost >= best) continue;
//                 dist[next.Data] = newCost;
//                 pq.Enqueue(next, newCost);
//             }
//         }
//
//         return -1;
//     }
//
//     public static void Main()
//     {
//         var lines = new List<string>();
//
//         while (Console.ReadLine() is { } line && line != "")
//             lines.Add(line);
//
//         Console.WriteLine(Solve(lines));
//
//     }
// }