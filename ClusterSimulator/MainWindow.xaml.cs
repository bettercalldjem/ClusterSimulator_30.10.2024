using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace JobSchedulerApp
{
    public partial class MainWindow : Window
    {
        private List<Node> nodes;
        private JobScheduler jobScheduler;
        private DispatcherTimer timer;
        private int jobCounter = 1;
        public MainWindow()
        {
            InitializeComponent();
            InitializeNodes();
            UpdateNodeListView();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void InitializeNodes()
        {
            nodes = new List<Node>
            {
                new Node(1, 100),
                new Node(2, 80),
                new Node(3, 120),
            };
            jobScheduler = new JobScheduler(nodes);
        }

        private void UpdateNodeListView()
        {
            NodesListView.ItemsSource = null;
            NodesListView.ItemsSource = nodes;
        }

        private void AddJobButton_Click(object sender, RoutedEventArgs e)
        {
            var job = new Job(jobCounter++, 20, 1, TimeSpan.FromSeconds(5));
            jobScheduler.ScheduleJob(job);
            UpdateNodeListView();
        }

        private void StartSimulationButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach (var node in nodes)
            {
                if (node.CurrentLoad > 0)
                {
                    var job = new Job(1, 20, 1, TimeSpan.FromSeconds(5));
                    jobScheduler.CompleteJob(node, job);
                }
            }
            UpdateNodeListView();
        }
    }

    public class Job
    {
        public int Id { get; set; }
        public int ResourceRequirement { get; set; }
        public int Priority { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public string Status { get; set; }

        public Job(int id, int resourceRequirement, int priority, TimeSpan executionTime)
        {
            Id = id;
            ResourceRequirement = resourceRequirement;
            Priority = priority;
            ExecutionTime = executionTime;
            Status = "Ожидание";
        }
    }

    public class Node
    {
        public int Id { get; set; }
        public int AvailableResources { get; set; }
        public int CurrentLoad { get; set; }
        public string Status { get; set; }

        public Node(int id, int availableResources)
        {
            Id = id;
            AvailableResources = availableResources;
            CurrentLoad = 0;
            Status = "Ожидание";
        }

        public bool CanExecuteJob(Job job)
        {
            return AvailableResources >= job.ResourceRequirement;
        }

        public void AddJob(Job job)
        {
            CurrentLoad += job.ResourceRequirement;
            AvailableResources -= job.ResourceRequirement;
            Status = CurrentLoad > AvailableResources * 0.8 ? "Перегрузка" : "Работаем";
        }

        public void CompleteJob(Job job)
        {
            CurrentLoad -= job.ResourceRequirement;
            AvailableResources += job.ResourceRequirement;
            Status = CurrentLoad == 0 ? "Ожидание" : "Работаем";
        }
    }

    public class JobScheduler
    {
        private List<Node> nodes;

        public JobScheduler(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        public void ScheduleJob(Job job)
        {
            var availableNode = nodes
                .Where(n => n.CanExecuteJob(job))
                .OrderBy(n => n.CurrentLoad)
                .FirstOrDefault();

            if (availableNode != null)
            {
                availableNode.AddJob(job);
                job.Status = "В процессе";
            }
            else
            {
                job.Status = "Ожидание";
            }
        }

        public void CompleteJob(Node node, Job job)
        {
            node.CompleteJob(job);
            job.Status = "Завершено";
        }
    }
}
