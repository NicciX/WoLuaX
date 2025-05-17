using System;
using System.Threading.Tasks;

namespace WoLuaX;

public class SingleExecutionTask {
	private readonly Action func;
	protected Task Task { get; private set; } = Task.CompletedTask;

	public bool Running {
		get {
			lock (this) {
				return Task.Status is TaskStatus.Running;
			}
		}
	}
	public bool Completed {
		get {
			lock (this) {
				return Task.IsCompleted;
			}
		}
	}

	public SingleExecutionTask(Action func) => this.func = func;

	public bool TryRun() {
		lock (this) {
			if (Completed) {
                Task = Task.Run(func);
				return true;
			}
		}
		return false;
	}
	public void Run() => TryRun();
}
