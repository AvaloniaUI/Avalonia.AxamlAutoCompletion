using System.Collections.Concurrent;

namespace Avalonia.AXAML.LanguageServer.ProjectModel
{
    public class ProjectShepard
    {
        ConcurrentDictionary<string, WorkspaceProject> _projects = new ConcurrentDictionary<string, WorkspaceProject>();

        public WorkspaceProject GetProject(string path)
        {
            return ProjectAdded(path);
        }

        public WorkspaceProject ProjectAdded(string path)
        {
            return _projects.GetOrAdd(path, p => new WorkspaceProject(p));
        }

        public void ProjectRemoved(string path)
        {
            _projects.TryRemove(path, out _);
        }

        internal IList<WorkspaceProject> GetProjectsByName(string name)
        {
            return _projects.Values.Where(n => n.Name.Equals(name)).ToList();
        }
    }
}
