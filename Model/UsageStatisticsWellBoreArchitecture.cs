using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NORCE.Drilling.WellBoreArchitecture.Model
{
    public struct CountPerDay
    {
        public DateTime Date { get; set; }
        public ulong Count { get; set; }

        public CountPerDay() { }

        public CountPerDay(DateTime date, ulong count)
        {
            Date = date;
            Count = count;
        }
    }

    public class History
    {
        public List<CountPerDay> Data { get; set; } = new List<CountPerDay>();

        public History()
        {
            if (Data == null)
            {
                Data = new List<CountPerDay>();
            }
        }

        public void Increment()
        {
            if (Data.Count == 0)
            {
                Data.Add(new CountPerDay(DateTime.UtcNow.Date, 1));
            }
            else if (Data[Data.Count - 1].Date < DateTime.UtcNow.Date)
            {
                Data.Add(new CountPerDay(DateTime.UtcNow.Date, 1));
            }
            else
            {
                Data[Data.Count - 1] = new CountPerDay(Data[Data.Count - 1].Date, Data[Data.Count - 1].Count + 1);
            }
        }
    }

    public class UsageStatisticsWellBoreArchitecture
    {
        public static readonly string HOME_DIRECTORY = ".." + Path.DirectorySeparatorChar + "home" + Path.DirectorySeparatorChar;

        public DateTime LastSaved { get; set; } = DateTime.MinValue;
        public TimeSpan BackUpInterval { get; set; } = TimeSpan.FromMinutes(5);

        public History GetAllWellBoreArchitectureIdPerDay { get; set; } = new History();
        public History GetAllWellBoreArchitectureMetaInfoPerDay { get; set; } = new History();
        public History GetWellBoreArchitectureByIdPerDay { get; set; } = new History();
        public History GetAllWellBoreArchitectureLightPerDay { get; set; } = new History();
        public History GetAllWellBoreArchitecturePerDay { get; set; } = new History();
        public History PostWellBoreArchitecturePerDay { get; set; } = new History();
        public History PutWellBoreArchitectureByIdPerDay { get; set; } = new History();
        public History DeleteWellBoreArchitectureByIdPerDay { get; set; } = new History();

        private static readonly object lock_ = new object();
        private static UsageStatisticsWellBoreArchitecture? instance_ = null;

        public static UsageStatisticsWellBoreArchitecture Instance
        {
            get
            {
                if (instance_ == null)
                {
                    if (File.Exists(HOME_DIRECTORY + "history.json"))
                    {
                        try
                        {
                            string? jsonStr = null;
                            lock (lock_)
                            {
                                using (StreamReader reader = new StreamReader(HOME_DIRECTORY + "history.json"))
                                {
                                    jsonStr = reader.ReadToEnd();
                                }

                                if (!string.IsNullOrEmpty(jsonStr))
                                {
                                    instance_ = JsonSerializer.Deserialize<UsageStatisticsWellBoreArchitecture>(jsonStr);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (instance_ == null)
                    {
                        instance_ = new UsageStatisticsWellBoreArchitecture();
                    }
                }

                return instance_;
            }
        }

        public void IncrementGetAllWellBoreArchitectureIdPerDay()
        {
            lock (lock_)
            {
                if (GetAllWellBoreArchitectureIdPerDay == null)
                {
                    GetAllWellBoreArchitectureIdPerDay = new History();
                }

                GetAllWellBoreArchitectureIdPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementGetAllWellBoreArchitectureMetaInfoPerDay()
        {
            lock (lock_)
            {
                if (GetAllWellBoreArchitectureMetaInfoPerDay == null)
                {
                    GetAllWellBoreArchitectureMetaInfoPerDay = new History();
                }

                GetAllWellBoreArchitectureMetaInfoPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementGetWellBoreArchitectureByIdPerDay()
        {
            lock (lock_)
            {
                if (GetWellBoreArchitectureByIdPerDay == null)
                {
                    GetWellBoreArchitectureByIdPerDay = new History();
                }

                GetWellBoreArchitectureByIdPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementGetAllWellBoreArchitectureLightPerDay()
        {
            lock (lock_)
            {
                if (GetAllWellBoreArchitectureLightPerDay == null)
                {
                    GetAllWellBoreArchitectureLightPerDay = new History();
                }

                GetAllWellBoreArchitectureLightPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementGetAllWellBoreArchitecturePerDay()
        {
            lock (lock_)
            {
                if (GetAllWellBoreArchitecturePerDay == null)
                {
                    GetAllWellBoreArchitecturePerDay = new History();
                }

                GetAllWellBoreArchitecturePerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementPostWellBoreArchitecturePerDay()
        {
            lock (lock_)
            {
                if (PostWellBoreArchitecturePerDay == null)
                {
                    PostWellBoreArchitecturePerDay = new History();
                }

                PostWellBoreArchitecturePerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementPutWellBoreArchitectureByIdPerDay()
        {
            lock (lock_)
            {
                if (PutWellBoreArchitectureByIdPerDay == null)
                {
                    PutWellBoreArchitectureByIdPerDay = new History();
                }

                PutWellBoreArchitectureByIdPerDay.Increment();
                ManageBackup();
            }
        }

        public void IncrementDeleteWellBoreArchitectureByIdPerDay()
        {
            lock (lock_)
            {
                if (DeleteWellBoreArchitectureByIdPerDay == null)
                {
                    DeleteWellBoreArchitectureByIdPerDay = new History();
                }

                DeleteWellBoreArchitectureByIdPerDay.Increment();
                ManageBackup();
            }
        }

        private void ManageBackup()
        {
            if (DateTime.UtcNow > LastSaved + BackUpInterval)
            {
                LastSaved = DateTime.UtcNow;
                try
                {
                    string jsonStr = JsonSerializer.Serialize(this);
                    if (!string.IsNullOrEmpty(jsonStr) && Directory.Exists(HOME_DIRECTORY))
                    {
                        using (StreamWriter writer = new StreamWriter(HOME_DIRECTORY + "history.json"))
                        {
                            writer.Write(jsonStr);
                            writer.Flush();
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
