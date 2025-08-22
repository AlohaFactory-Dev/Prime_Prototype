using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Durian
{
    public class DurianDaemon : ITickable
    {
        private class Job
        {
            public string id;
            public Func<UniTask> task;
            public bool isRunning;
            public float interval;
            public float timer;
        }

        private List<Job> _jobs = new List<Job>();

        // 등록 후 interval만큼 지난 다음에 첫번째 실행됨
        public void Register(string id, Func<UniTask> task, float interval)
        {
            Assert.IsFalse(_jobs.Exists(job => job.id == id), $"DurianDaemon :: Job with id {id} already exists");
            _jobs.Add(new Job { id = id, task = task, interval = interval, timer = interval });
        }

        public void Remove(string id)
        {
            _jobs.RemoveAll(job => job.id == id);
        }

        public void Tick()
        {
            foreach (var job in _jobs)
            {
                if (job.isRunning) continue;
                
                job.timer += Time.deltaTime;
                if (job.timer >= job.interval)
                {
                    Debug.Log($"DurianDaemon :: Run job {job.id}");
                    job.isRunning = true;
                    job.timer = 0;
                    job.task().ContinueWith(() => job.isRunning = false).Forget();
                }
            }
        }
    }
}
