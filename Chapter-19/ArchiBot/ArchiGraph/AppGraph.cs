using System.Collections.Generic;

namespace ArchiBot.ArchiGraph
{
    public class AppGraph
    {
        public List<Edge> Edges { get; set; }
        public List<Node> Nodes { get; set; }

        public AppGraph()
        {
            Edges = new List<Edge>();
            Nodes = new List<Node>();
        }
    }
}
