namespace Lab8.Green
{
    public class Task2
    {
        public class Human
        {
            private string _name;
            private string _surname;

            public Human(string name, string surname)
            {
                _name = name == null ? "" : name;
                _surname = surname == null ? "" : surname;
            }

            public string Name { get { return _name; } }
            public string Surname { get { return _surname; } }

            public void Print()
            {
                System.Console.WriteLine($"{Name} {Surname}");
            }
        }

        public class Student : Human
        {
            private int[] _marks;
            private static int _excellentAmount;

            static Student()
            {
                _excellentAmount = 0;
            }

            public Student(string name, string surname)
                : base(name, surname)
            {
                _marks = new int[4];
            }

            public int[] Marks
            {
                get
                {
                    if (_marks == null)
                        return new int[0];

                    int[] copy = new int[_marks.Length];
                    for (int i = 0; i < _marks.Length; i++)
                        copy[i] = _marks[i];
                    return copy;
                }
            }

            public double AverageMark
            {
                get
                {
                    if (_marks == null || _marks.Length == 0)
                        return 0;

                    bool allZero = true;
                    for (int i = 0; i < _marks.Length; i++)
                    {
                        if (_marks[i] != 0)
                        {
                            allZero = false;
                            break;
                        }
                    }

                    if (allZero)
                        return 0;

                    int sum = 0;
                    for (int i = 0; i < _marks.Length; i++)
                        sum += _marks[i];

                    return (double)sum / _marks.Length;
                }
            }

            public bool IsExcellent
            {
                get
                {
                    if (_marks == null || _marks.Length == 0)
                        return false;

                    for (int i = 0; i < _marks.Length; i++)
                    {
                        if (_marks[i] == 0)
                            return false;
                    }

                    for (int i = 0; i < _marks.Length; i++)
                    {
                        if (_marks[i] < 4)
                            return false;
                    }

                    return true;
                }
            }

            public static int ExcellentAmount { get { return _excellentAmount; } }

            public void Exam(int mark)
            {
                if (_marks == null)
                    return;

                for (int i = 0; i < _marks.Length; i++)
                {
                    if (_marks[i] == 0)
                    {
                        _marks[i] = mark;

                        if (IsExcellent)
                            _excellentAmount++;

                        return;
                    }
                }
            }

            public static void SortByAverageMark(Student[] array)
            {
                if (array == null)
                    return;

                for (int i = 0; i < array.Length - 1; i++)
                {
                    for (int j = 0; j < array.Length - 1 - i; j++)
                    {
                        double left = array[j] == null ? -1 : array[j].AverageMark;
                        double right = array[j + 1] == null ? -1 : array[j + 1].AverageMark;

                        if (left < right)
                        {
                            Student temp = array[j];
                            array[j] = array[j + 1];
                            array[j + 1] = temp;
                        }
                    }
                }
            }

            public new void Print()
            {
                System.Console.WriteLine($"{Name} {Surname} {AverageMark} {IsExcellent}");
            }
        }
    }
}
