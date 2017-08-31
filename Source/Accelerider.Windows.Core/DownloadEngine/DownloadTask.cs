﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Accelerider.Windows.Infrastructure;
using Accelerider.Windows.Infrastructure.Interfaces;

namespace Accelerider.Windows.Core.DownloadEngine
{
    internal class DownloadTask : ITransferTaskToken
    {
        public DownloadTaskItem Item { get; }

        internal DownloadTask(DownloadTaskItem item)
        {
            Item = item;
            DownloadTaskManager.Manager.TaskStateChangeEvent += Manager_TaskStateChangeEvent;
        }

        private void Manager_TaskStateChangeEvent(DownloadTaskItem arg1, TransferTaskStatusEnum arg2, TransferTaskStatusEnum arg3)
        {
            if (arg1 == Item)
            {
                TransferTaskStatusChanged?.Invoke(this, new TransferTaskStatusChangedEventArgs(this, arg2, arg3));
            }
        }

        public event EventHandler<TransferTaskStatusChangedEventArgs> TransferTaskStatusChanged;

        public TransferTaskStatusEnum TransferTaskStatus => DownloadTaskManager.Manager.GetTaskProcess(Item)?.DownloadState ??
                                                  TransferTaskStatusEnum.Canceled;

        public string OwnerName
        {
            get { throw new NotImplementedException(); }
        }

        public IDiskFile FileInfo => Item.NetDiskFile;

        public DataSize Progress => new DataSize(
            DownloadTaskManager.Manager.GetTaskProcess(Item)?.Info.CompletedLength ?? 0L);

        public async Task<bool> PauseAsync()
        {
            return await Task.Run(() =>
            {
                var task = DownloadTaskManager.Manager.GetTaskProcess(Item);
                if (task == null) return false;
                task.StopAndSave();
                return true;
            });
        }

        public async Task<bool> StartAsync(bool force = false)
        {
            return await Task.Run(() => false);
        }

        public Task<bool> CancelAsync()
        {
            throw new NotImplementedException();
        }

        public bool Equals(ITransferTaskToken other) => FileInfo.FilePath == other?.FileInfo.FilePath;
    }
}
