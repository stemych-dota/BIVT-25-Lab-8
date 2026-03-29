using System.Reflection;
using System.Text.Json;

namespace Lab8Test.Green
{
   [TestClass]
   public sealed class Task4
   {
       record InputRow(string Name, string Surname, double[] Jumps);
       record OutputRow(string Name, string Surname, double BestJump);

       private InputRow[] _input;
       private OutputRow[] _outputSorted;

       private Lab8.Green.Task4.Participant[] _participants;
       private Lab8.Green.Task4.Discipline _longJump;
       private Lab8.Green.Task4.Discipline _highJump;

       [TestInitialize]
       public void LoadData()
       {
           var folder = Directory.GetParent(Directory.GetCurrentDirectory())
               .Parent.Parent.Parent.FullName;
           folder = Path.Combine(folder, "Lab8Test", "Green");

           var input = JsonSerializer.Deserialize<JsonElement>(
               File.ReadAllText(Path.Combine(folder, "input.json")))!;
           var output = JsonSerializer.Deserialize<JsonElement>(
               File.ReadAllText(Path.Combine(folder, "output.json")))!;

           _input = input.GetProperty("Task4").Deserialize<InputRow[]>()!;
           _outputSorted = output.GetProperty("Task4").GetProperty("Sorted").Deserialize<OutputRow[]>()!;

           _participants = new Lab8.Green.Task4.Participant[_input.Length];
           _longJump = new Lab8.Green.Task4.LongJump();
           _highJump = new Lab8.Green.Task4.HighJump();
       }
       [TestMethod]
       public void Test_00_OOP()
       {
           var participant = typeof(Lab8.Green.Task4.Participant);
           var discipline = typeof(Lab8.Green.Task4.Discipline);
           var longJump = typeof(Lab8.Green.Task4.LongJump);
           var highJump = typeof(Lab8.Green.Task4.HighJump);

           Assert.IsTrue(participant.IsValueType);
           Assert.AreEqual(0, participant.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);

           Assert.IsTrue(participant.GetProperty("Name")?.CanRead ?? false);
           Assert.IsTrue(participant.GetProperty("Surname")?.CanRead ?? false);
           Assert.IsTrue(participant.GetProperty("Jumps")?.CanRead ?? false);
           Assert.IsTrue(participant.GetProperty("BestJump")?.CanRead ?? false);

           Assert.IsFalse(participant.GetProperty("Name")?.CanWrite ?? true);
           Assert.IsFalse(participant.GetProperty("Surname")?.CanWrite ?? true);
           Assert.IsFalse(participant.GetProperty("Jumps")?.CanWrite ?? true);
           Assert.IsFalse(participant.GetProperty("BestJump")?.CanWrite ?? true);

           Assert.IsNotNull(participant.GetConstructor(
               BindingFlags.Instance | BindingFlags.Public,
               null,
               new[] { typeof(string), typeof(string) },
               null));

           Assert.IsNotNull(participant.GetMethod("Jump"));
           Assert.IsNotNull(participant.GetMethod("Sort", BindingFlags.Static | BindingFlags.Public));
           Assert.IsNotNull(participant.GetMethod("Print"));

           Assert.AreEqual(participant.GetConstructors().Count(c => c.IsPublic), 1);
           Assert.IsTrue(participant.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length >= 2);

           Assert.IsTrue(discipline.IsClass);
           Assert.IsTrue(discipline.IsAbstract);

           Assert.AreEqual(0, discipline.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);

           Assert.IsTrue(discipline.GetProperty("Name")?.CanRead ?? false);
           Assert.IsTrue(discipline.GetProperty("Participants")?.CanRead ?? false);

           Assert.IsNotNull(discipline.GetMethod("Add", new[] { typeof(Lab8.Green.Task4.Participant) }));
           Assert.IsNotNull(discipline.GetMethod("Add", new[] { typeof(Lab8.Green.Task4.Participant[]) }));
           Assert.IsNotNull(discipline.GetMethod("Sort"));
           Assert.IsNotNull(discipline.GetMethod("Retry"));
           Assert.IsNotNull(discipline.GetMethod("Print"));

           Assert.AreEqual(0, discipline.GetConstructors().Count(c => c.IsPublic));
           Assert.AreEqual(discipline.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 11);
           Assert.AreEqual(discipline.GetProperties().Count(p => p.PropertyType.IsPublic), 2);

           Assert.IsTrue(longJump.IsSubclassOf(discipline));
           Assert.IsNotNull(longJump.GetConstructor(Type.EmptyTypes));

           Assert.AreEqual(0, longJump.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);
           Assert.AreEqual(longJump.GetConstructors().Count(c => c.IsPublic), 1);
           Assert.AreEqual(longJump.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 11);
           Assert.AreEqual(longJump.GetProperties().Count(p => p.PropertyType.IsPublic), 2);

           Assert.IsTrue(highJump.IsSubclassOf(discipline));
           Assert.IsNotNull(highJump.GetConstructor(Type.EmptyTypes));

           Assert.AreEqual(0, highJump.GetFields(BindingFlags.Public | BindingFlags.Instance).Length);
           Assert.AreEqual(highJump.GetConstructors().Count(c => c.IsPublic), 1);
           Assert.AreEqual(highJump.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 11);
           Assert.AreEqual(highJump.GetProperties().Count(p => p.PropertyType.IsPublic), 2);
       }


       [TestMethod]
       public void Test_01_CreateParticipants()
       {
           InitParticipants();

           for (int i = 0; i < _participants.Length; i++)
           {
               Assert.AreEqual(_input[i].Name, _participants[i].Name);
               Assert.AreEqual(_input[i].Surname, _participants[i].Surname);
               Assert.AreEqual(0, _participants[i].BestJump, 0.0001);
           }
       }

       [TestMethod]
       public void Test_02_Jumps()
       {
           InitParticipants();
           ApplyJumps();

           for (int i = 0; i < _participants.Length; i++)
           {
               Assert.AreEqual(_input[i].Jumps.Max(), _participants[i].BestJump, 0.0001);
           }
       }

       [TestMethod]
       public void Test_03_ArrayIsolation()
       {
           InitParticipants();
           ApplyJumps();

           for (int i = 0; i < _participants.Length; i++)
           {
               var arr = _participants[i].Jumps;
               for (int j = 0; j < arr.Length; j++)
                   arr[j] = -1;
           }

           for (int i = 0; i < _participants.Length; i++)
           {
               Assert.AreEqual(_input[i].Jumps.Max(), _participants[i].BestJump, 0.0001);
           }
       }

       [TestMethod]
       public void Test_04_DisciplineAddAndSort()
       {
           InitParticipants();
           ApplyJumps();

           _longJump.Add(_participants);
           _longJump.Sort();

           var arr = _longJump.Participants;

           for (int i = 0; i < arr.Length; i++)
           {
               Assert.AreEqual(_outputSorted[i].Name, arr[i].Name);
               Assert.AreEqual(_outputSorted[i].Surname, arr[i].Surname);
               Assert.AreEqual(_outputSorted[i].BestJump, arr[i].BestJump, 0.0001);
           }
       }

       [TestMethod]
       public void Test_05_LongJumpRetry()
       {
           InitParticipants();
           ApplyJumps();

           _longJump.Add(_participants);

           int idx = 0;
           double best = _participants[idx].BestJump;
           var before = new double[3];
           Array.Copy(_participants[idx].Jumps, before, 3);

           _longJump.Retry(idx);
           _longJump.Participants[idx].Jump(before[0]);
           _longJump.Participants[idx].Jump(before[1]);


           Assert.AreEqual(best, _longJump.Participants[idx].Jumps[0], 0.0001);
           Assert.AreEqual(before[0], _longJump.Participants[idx].Jumps[1], 0.0001);
           Assert.AreEqual(before[1], _longJump.Participants[idx].Jumps[2], 0.0001);
       }

       [TestMethod]
       public void Test_06_HighJumpRetry()
       {
           InitParticipants();
           ApplyJumps();

           _highJump.Add(_participants);

           int idx = 0;
           var before = _participants[idx].Jumps;

           _highJump.Retry(idx);
           _highJump.Participants[idx].Jump(10);

           Assert.AreEqual(before[0], _highJump.Participants[idx].Jumps[0], 0.0001);
           Assert.AreEqual(before[1], _highJump.Participants[idx].Jumps[1], 0.0001);
           Assert.AreEqual(10, _highJump.Participants[idx].Jumps[2], 0.0001);
       }
       private void ResetAllParticipantStatics()
       {
           var baseType = typeof(Lab8.Green.Task4.Discipline);

           var allTypes = baseType.Assembly
               .GetTypes()
               .Where(t => baseType.IsAssignableFrom(t));

           foreach (var type in allTypes)
           {
               var staticFields = type.GetFields(
                   BindingFlags.Static |
                   BindingFlags.Public |
                   BindingFlags.NonPublic);

               foreach (var field in staticFields)
               {
                   if (field.FieldType == typeof(int))
                       field.SetValue(null, 0);
                   else if (field.FieldType == typeof(double))
                       field.SetValue(null, 0.0);
                   else if (field.FieldType == typeof(bool))
                       field.SetValue(null, false);
                   else
                       field.SetValue(null, null);
               }
           }
       }
       private void InitParticipants()
       {
           ResetAllParticipantStatics();
           for (int i = 0; i < _input.Length; i++)
           {
               _participants[i] = new Lab8.Green.Task4.Participant(
                   _input[i].Name,
                   _input[i].Surname);
           }
       }

       private void ApplyJumps()
       {
           for (int i = 0; i < _input.Length; i++)
               foreach (var j in _input[i].Jumps)
                   _participants[i].Jump(j);
       }
   }
}
