﻿using System;
using System.Collections.Generic;
using System.Timers;
using Timer = System.Timers.Timer;

public delegate void TimeEvent();
public class ScheduleUtil
{
    private static ScheduleUtil util;
    public static ScheduleUtil Instance { get { if (util == null) { util = new ScheduleUtil(); } return util; } }
    System.Timers.Timer timer;

    private ConcurrentInteger index = new ConcurrentInteger();

    // 等待执行的任务表
    private Dictionary<int, TimeTaskModel> mission = new Dictionary<int, TimeTaskModel>();

    // 等待移除的任务列表
    private List<int> removelist = new List<int>();

    private ScheduleUtil()
    {
        timer = new System.Timers.Timer(14);
        timer.Elapsed += callback;
        timer.Start();
    }

    void callback(object sender, ElapsedEventArgs e)
    {
        lock (mission)
        {
            lock (removelist)
            {
                foreach (int item in removelist)
                {
                    mission.Remove(item);
                }
                removelist.Clear();
                foreach (TimeTaskModel item in mission.Values)
                {
                    if (item.time <= DateTime.Now.Ticks)
                    {
                        item.run();
                        removelist.Add(item.id);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 任务调用  毫秒
    /// </summary>
    /// <param name="task"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public int schedule(TimeEvent task, long delay)
    {
        //毫秒转微秒
        return schedulemms(task, delay * 1000 * 1000);
    }
    /// <summary>
    /// 微秒级时间轴
    /// </summary>
    /// <param name="task"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private int schedulemms(TimeEvent task, long delay)
    {
        lock (mission)
        {
            int id = index.GetAndAdd();
            TimeTaskModel model = new TimeTaskModel(id, task, DateTime.Now.Ticks + delay);
            mission.Add(id, model);
            return id;
        }
    }

    public void removeMission(int id)
    {
        lock (removelist)
        {
            removelist.Add(id);
        }
    }


    public int schedule(TimeEvent task, DateTime time)
    {
        long t = time.Ticks - DateTime.Now.Ticks;
        t = Math.Abs(t);
        return schedulemms(task, t);
    }


    public int timeSchedule(TimeEvent task, long time)
    {
        long t = time - DateTime.Now.Ticks;
        t = Math.Abs(t);
        return schedulemms(task, t);
    }
}