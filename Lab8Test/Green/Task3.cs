using System.Reflection;
using System.Security.AccessControl;
using System.Text.Json;

namespace Lab8Test.Green
{
   [TestClass]
   public sealed class Task3
   {
       record InputRow(string Name, string Surname, int[] Marks);
       record OutputRow(string Name, string Surname, double AverageMark, bool IsExpelled, int ID);

       private InputRow[] _input;
       private OutputRow[] _outputOriginal;
       private OutputRow[] _outputSorted;
       private Lab8.Green.Task3.Student[] _students;

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

           _input = input.GetProperty("Task3").Deserialize<InputRow[]>();
           _outputOriginal = output.GetProperty("Task3").GetProperty("Original").Deserialize<OutputRow[]>();
           _outputSorted = output.GetProperty("Task3").GetProperty("Sorted").Deserialize<OutputRow[]>();

           _students = new Lab8.Green.Task3.Student[_input.Length];
       }
       [TestMethod]
       public void Test_00_OOP()
       {
           var type = typeof(Lab8.Green.Task3.Student);

           Assert.AreEqual(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Length, 0);
           Assert.IsTrue(type.IsClass);

           Assert.IsTrue(type.GetProperty("Name")?.CanRead ?? false);
           Assert.IsTrue(type.GetProperty("Surname")?.CanRead ?? false);
           Assert.IsTrue(type.GetProperty("AverageMark")?.CanRead ?? false);
           Assert.IsTrue(type.GetProperty("IsExpelled")?.CanRead ?? false);
           Assert.IsTrue(type.GetProperty("Marks")?.CanRead ?? false);
           Assert.IsTrue(type.GetProperty("ID")?.CanRead ?? false);

           Assert.IsFalse(type.GetProperty("Name")?.CanWrite ?? true);
           Assert.IsFalse(type.GetProperty("Surname")?.CanWrite ?? true);
           Assert.IsFalse(type.GetProperty("AverageMark")?.CanWrite ?? true);
           Assert.IsFalse(type.GetProperty("IsExpelled")?.CanWrite ?? true);
           Assert.IsFalse(type.GetProperty("Marks")?.CanWrite ?? true);
           Assert.IsFalse(type.GetProperty("ID")?.CanWrite ?? true);

           Assert.IsNotNull(type.GetConstructor(
               BindingFlags.Instance | BindingFlags.Public,
               null,
               new[] { typeof(string), typeof(string) },
               null));

           Assert.IsNotNull(type.GetMethod("Exam", BindingFlags.Instance | BindingFlags.Public));
           Assert.IsNotNull(type.GetMethod("Restore", BindingFlags.Instance | BindingFlags.Public));
           Assert.IsNotNull(type.GetMethod("SortByAverageMark", BindingFlags.Static | BindingFlags.Public));
           Assert.IsNotNull(type.GetMethod("Print", BindingFlags.Instance | BindingFlags.Public));

           Assert.AreEqual(0, type.GetFields().Count(f => f.IsPublic));
           Assert.AreEqual(type.GetProperties().Count(f => f.PropertyType.IsPublic), 6);
           Assert.AreEqual(type.GetConstructors().Count(f => f.IsPublic), 1);
           Assert.AreEqual(type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 13);

           var commission = typeof(Lab8.Green.Task3.Commission);
           Assert.IsTrue(commission.IsClass);

           Assert.IsNotNull(commission.GetMethod("Sort", BindingFlags.Static | BindingFlags.Public));
           Assert.IsNotNull(commission.GetMethod("Expel", BindingFlags.Static | BindingFlags.Public));
           Assert.IsNotNull(commission.GetMethod("Restore", BindingFlags.Static | BindingFlags.Public));

           Assert.AreEqual(0, commission.GetFields().Count(f => f.IsPublic));
           Assert.AreEqual(commission.GetProperties().Count(f => f.PropertyType.IsPublic), 0);
           Assert.AreEqual(commission.GetConstructors().Count(f => f.IsPublic), 1);
           Assert.AreEqual(type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Length, 13);
       }

       [TestMethod]
       public void Test_02_CreateStudents()
       {
           InitStudents();
           for (int i = 0; i < _students.Length; i++)
           {
               Assert.AreEqual(_input[i].Name, _students[i].Name);
               Assert.AreEqual(_input[i].Surname, _students[i].Surname);
               Assert.AreEqual(0, _students[i].AverageMark, 0.0001);
               Assert.IsFalse(_students[i].IsExpelled);
               Assert.AreEqual(i + 1, _students[i].ID);
           }
       }

       [TestMethod]
       public void Test_03_ExamStudents()
       {
           InitStudents();
           ApplyExams();

           for (int i = 0; i < _students.Length; i++)
           {
               var marks = _input[i].Marks.Where(m => m > 0).ToArray();
               double avg = marks.Length == 0 ? 0 : marks.Average();
               bool expelled = _input[i].Marks.Any(m => m == 2);

               Assert.AreEqual(avg, _students[i].AverageMark, 0.0001);
               Assert.AreEqual(expelled, _students[i].IsExpelled);
           }
       }

       [TestMethod]
       public void Test_04_SortByAverageMark()
       {
           InitStudents();
           ApplyExams();

           Lab8.Green.Task3.Student.SortByAverageMark(_students);

           for (int i = 0; i < _students.Length; i++)
           {
               Assert.AreEqual(_outputSorted[i].Name, _students[i].Name);
               Assert.AreEqual(_outputSorted[i].Surname, _students[i].Surname);
               Assert.AreEqual(_outputSorted[i].AverageMark, _students[i].AverageMark, 0.0001);
               Assert.AreEqual(_outputSorted[i].IsExpelled, _students[i].IsExpelled);
           }
       }

       [TestMethod]
       public void Test_05_CommissionSortByID()
       {
           InitStudents();
           ApplyExams();
           int startId = _students[0].ID;
           var shuffled = _students.ToArray();
           Array.Reverse(shuffled);
           Lab8.Green.Task3.Commission.Sort(shuffled);

           for (int i = 0; i < shuffled.Length; i++)
           {
               Assert.AreEqual(i + startId, shuffled[i].ID);
           }
       }

       [TestMethod]
       public void Test_06_CommissionExpel()
       {
           InitStudents();
           ApplyExams();

           var expelled = Lab8.Green.Task3.Commission.Expel(ref _students);

           foreach (var s in expelled)
           {
               Assert.IsTrue(s.IsExpelled);
           }
           foreach (var s in _students)
           {
               Assert.IsFalse(s.IsExpelled);
           }
       }

       [TestMethod]
       public void Test_07_CommissionRestore()
       {
           InitStudents();
           ApplyExams();

           var expelled = Lab8.Green.Task3.Commission.Expel(ref _students);
           if (expelled.Length > 0)
           {
               Lab8.Green.Task3.Commission.Restore(ref _students, expelled[0]);
               Assert.IsTrue(_students.Any(s => s.ID == expelled[0].ID));
           }
       }

       [TestMethod]
       public void Test_08_RestoreMethod_Student()
       {
           InitStudents();
           ApplyExams();

           var s = _students.FirstOrDefault(st => st.IsExpelled);
           if (s != null)
           {
               s.Restore();
               Assert.IsFalse(s.IsExpelled);
           }
       }

       private void ResetAllParticipantStatics()
       {
           var baseType = typeof(Lab8.Green.Task3.Student);

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
                       field.SetValue(null, 1);
                   else if (field.FieldType == typeof(double))
                       field.SetValue(null, 0.0);
                   else if (field.FieldType == typeof(bool))
                       field.SetValue(null, false);
                   else
                       field.SetValue(null, null);
               }
           }
       }
       private void InitStudents()
       {
           ResetAllParticipantStatics();
           for (int i = 0; i < _input.Length; i++)
           {
               _students[i] = new Lab8.Green.Task3.Student(_input[i].Name, _input[i].Surname);
           }
       }

       private void ApplyExams()
       {
           for (int i = 0; i < _input.Length; i++)
           {
               foreach (var mark in _input[i].Marks)
               {
                   _students[i].Exam(mark);
               }
           }
       }
   }
}
