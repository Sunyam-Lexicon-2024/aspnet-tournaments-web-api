using Microsoft.AspNetCore.JsonPatch;

namespace Tournaments.Test.Factories;

public class JsonPatchDocumentFactory
{

    public static JsonPatchDocument<Tournament> GenerateTournamentPatchDocument()
    {
        JsonPatchDocument<Tournament> patchDocument = new();

        patchDocument.Add(t => t.StartDate, new DateOnly(2024, 3, 3));

        return patchDocument;
    }
    
    public static JsonPatchDocument<Game> GenerateGamePatchDocument()
    {
        JsonPatchDocument<Game> patchDocument = new();

        patchDocument.Add(t => t.StartTime, new DateTime(2024, 3, 3));

        return patchDocument;
    }
}