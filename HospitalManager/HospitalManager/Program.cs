using System.Net.Http.Json;
using HospitalManager;

class Program
{
    private static readonly HttpClient HttpClient = new();

    private static readonly List<string> LastNames = new()
    {
        "Ivanov", "Petrov", "Sidorov", "Smirnov", "Kuznetsov",
        "Popov", "Sokolov", "Lebedev", "Kozlov", "Fedorov"
    };

    private static readonly List<string> FirstNames = new()
    {
        "Ivan", "Petr", "Sidr", "Alexey", "Nikolay",
        "Vladimir", "Sergey", "Andrey", "Dmitry", "Oleg"
    };

    private static readonly List<string> MiddleNames = new()
    {
        "Ivanovich", "Petrovich", "Sidorovich", "Alexeyevich", "Nikolayevich",
        "Vladimirovich", "Sergeevich", "Andreevich", "Dmitrievich", "Olegovich"
    };


    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a base URL as a command-line argument.");
            return;
        }

        var baseUrl = args[0];
        HttpClient.BaseAddress = new Uri(baseUrl);

        var patients = new List<Patient>();

        for (var i = 0; i < 100; i++)
        {
            patients.Add(GenerateRandomPatient(i));
        }

        await PostPatients(patients);

        Console.WriteLine("Finished adding patients.");
    }

    private static Patient GenerateRandomPatient(int index)
    {
        var random = new Random();

        return new Patient
        {
            Name = new Name
            {
                Family = LastNames[random.Next(LastNames.Count)],
                Use = "official",
                Given = new[]
                {
                    FirstNames[random.Next(FirstNames.Count)],
                    MiddleNames[random.Next(MiddleNames.Count)]
                }
            },
            Gender = (Gender)(index % 4),
            BirthDate = DateTime.UtcNow.AddYears(-index),
            Active = index % 2 == 0
        };
    }

    private static async Task PostPatients(List<Patient> patients)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("/patients/batch", patients);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding patients: {ex.Message}");
        }
    }
}
