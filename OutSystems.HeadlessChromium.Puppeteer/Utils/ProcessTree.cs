using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OutSystems.HeadlessChromium.Puppeteer.Utils {
    public class ProcessTree {

        private readonly Process root;


        public class Node {

            public readonly Process Process;
            protected readonly ICollection<Node> children = new List<Node>();

            protected Node(Process process) {
                this.Process = process;
            }


            public IEnumerable<Node> Children { get { return children; } }

            public int ProcessCount {
                get {
                    int count = 1;
                    foreach (var child in children) {
                        count += child.ProcessCount;
                    }

                    return count;
                }
            }

            public bool Contains(Process process) {
                if (this.Process.Id == process.Id) {
                    return true;
                }

                foreach (var child in children) {
                    if (child.Contains(process)) {
                        return true;
                    }
                }

                return false;
            }

        }

        private class NodeInstance : Node {

            public NodeInstance(Process process) : base(process) {
            }

            public void AddChildNode(Node child) {
                children.Add(child);
            }
        }



        public ProcessTree(Process root) {
            this.root = root;
        }


        public Node GetProcessTree() {
            var map = GetParentProcessesMap();

            var treeRoot = new NodeInstance(root);
            var nextChildren = new List<NodeInstance>();
            nextChildren.Add(treeRoot);

            while (nextChildren.Count != 0) {
                nextChildren = CollectNextChildProcesses(map, nextChildren);
            }

            return treeRoot;
        }


        private IDictionary<int, List<Process>> GetParentProcessesMap() {
            var map = new Dictionary<int, List<Process>>();
            Process[] allProcesses = Process.GetProcesses();

            foreach (var process in allProcesses) {
                var parent = GetParentProcess(process);
                if (parent != null) {
                    List<Process> entry;
                    if (!map.TryGetValue(parent.Id, out entry)) {
                        entry = new List<Process>();
                        map.Add(parent.Id, entry);
                    }

                    entry.Add(process);
                }
            }

            return map;
        }


        private List<NodeInstance> CollectNextChildProcesses(IDictionary<int, List<Process>> map, List<NodeInstance> parents) {
            var nextChildren = new List<NodeInstance>();

            foreach (var parent in parents) {
                List<Process> children;
                if (map.TryGetValue(parent.Process.Id, out children)) {
                    foreach (var child in children) {
                        var childNode = new NodeInstance(child);
                        parent.AddChildNode(childNode);
                        nextChildren.Add(childNode);
                    }
                }
            }

            return nextChildren;
        }









        // Adapted from https://stackoverflow.com/questions/394816/how-to-get-parent-process-in-net-in-managed-way

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION {

            // These members must match PROCESS_BASIC_INFORMATION
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;

            [DllImport("ntdll.dll")]
            public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);

        }

        public static Process GetParentProcess(Process process) {
            try {
                return GetParentProcess(process.Handle, process.StartTime);
            } catch {
                // not found or couldn't be reliably determined
                return null;
            }
        }


        private static Process GetParentProcess(IntPtr handle, DateTime startTime) {
            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
            int status = PROCESS_BASIC_INFORMATION.NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out _);
            if (status != 0) {
                throw new InvalidOperationException();
            }

            var parent = Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
            if (parent.StartTime <= startTime) {
                return parent;
            } else {
                // Windows might recycle process ids, so there is a possibility of the parent
                // process exiting, and its id being reused on a new process
                return null;
            }
        }

    }
}
