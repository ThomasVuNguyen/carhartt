using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Carhartt.Core
{
    public class MetricsService
    {
        private class ProcessState
        {
            public DateTime LastCheck;
            public TimeSpan LastTotalProcessorTime;
        }

        private readonly ConcurrentDictionary<int, ProcessState> _states = new();

        public (double MemoryMb, double CpuPercent) GetMetrics(IEnumerable<int> pids)
        {
            double totalMem = 0;
            double totalCpu = 0;
            
            var currentPids = new HashSet<int>(pids);
            // Add current process
            currentPids.Add(Process.GetCurrentProcess().Id);

            // Cleanup old states
            foreach (var pid in _states.Keys)
            {
                if (!currentPids.Contains(pid)) _states.TryRemove(pid, out _);
            }

            foreach (var pid in currentPids)
            {
                try
                {
                    using var p = Process.GetProcessById(pid); // Note: Dispose calls are tricky with GetProcessById if we want to keep it fast, but using blocks are safe.
                    // Actually, getting a new Process object every time is potentially expensive but safe.
                    
                    DateTime now = DateTime.UtcNow;
                    TimeSpan cpuTime = p.TotalProcessorTime; // Privileged? Usually OK for owned processes.
                    
                    if (_states.TryGetValue(pid, out var state))
                    {
                        double cpuUsedMs = (cpuTime - state.LastTotalProcessorTime).TotalMilliseconds;
                        double totalMsPassed = (now - state.LastCheck).TotalMilliseconds;
                        
                        if (totalMsPassed > 0)
                        {
                            double cpuUsage = (cpuUsedMs / totalMsPassed) / Environment.ProcessorCount * 100;
                            // Actually Process CPU % usually isn't divided by ProcessorCount in Task Manager (it can go >100%), 
                            // BUT typically people expect 0-100% normalized or 0-N*100%. 
                            // Let's normalize by ProcessorCount to give "System % used by this process".
                            // Wait, Task Manager Details shows 00-99 (summing to 100).
                            
                            totalCpu += (cpuUsedMs / totalMsPassed) * 100;
                        }

                        state.LastCheck = now;
                        state.LastTotalProcessorTime = cpuTime;
                    }
                    else
                    {
                        _states[pid] = new ProcessState { LastCheck = now, LastTotalProcessorTime = cpuTime };
                    }

                    totalMem += p.WorkingSet64;
                }
                catch
                {
                    // Process exited or access denied
                    _states.TryRemove(pid, out _);
                }
            }

            // Divide by core count if we want "Task Manager %" (e.g. 100% = full system load).
            // Currently totalCpu is sum of "Core %". e.g. 1 thread 100% on 4 core machine = 100.0 with above formula.
            // If we want " System %", we divide by Environment.ProcessorCount.
            totalCpu /= Environment.ProcessorCount;

            return (totalMem / 1024.0 / 1024.0, totalCpu);
        }
    }
}
