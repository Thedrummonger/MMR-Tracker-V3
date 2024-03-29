﻿using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class UndoRedo
    {
        public static void SaveState(this InstanceContainer Container, string _State = null)
        {
            int MaxUndos = Container.Instance.StaticOptions.OptionFile.MaxUndo;
            if (MaxUndos == 0)
            {
                Container.UndoStringList.Clear();
                Container.RedoStringList.Clear();
                return;
            }
            string State = _State is null ? Container.Instance.ToJson(JSONType.UTF8) : _State;
            Container.RedoStringList.Clear();
            Container.UndoStringList.Add(State);
            int AmountOverMax = Container.UndoStringList.Count - MaxUndos;
            if (AmountOverMax > 0) { Container.UndoStringList.RemoveRange(0, AmountOverMax); }
        }

        public static void DoUndo(this InstanceContainer Container)
        {
            if (Container.UndoStringList.Count == 0) { return; }
            string CurrentState = Container.Instance.ToJson(JSONType.UTF8);
            Container.LoadSerializedInstance(Container.UndoStringList[^1]);
            Container.RedoStringList.Add(CurrentState);
            Container.UndoStringList.RemoveAt(Container.UndoStringList.Count - 1);
        }
        public static void DoRedo(this InstanceContainer Container)
        {
            if (Container.RedoStringList.Count == 0) { return; }
            string CurrentState = Container.Instance.ToJson(JSONType.UTF8);
            Container.LoadSerializedInstance(Container.RedoStringList[^1]);
            Container.UndoStringList.Add(CurrentState);
            Container.RedoStringList.RemoveAt(Container.RedoStringList.Count - 1);
        }
    }
}
