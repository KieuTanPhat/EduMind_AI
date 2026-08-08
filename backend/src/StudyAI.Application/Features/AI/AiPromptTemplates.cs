namespace StudyAI.Application.Features.AI;

internal static class AiPromptTemplates
{
    public const string Summary = "Summarize the supplied study document in Vietnamese using clear Markdown. Include: overview, key concepts, important definitions, and a short review checklist. Do not invent facts that are not present in the document.";

    public const string MindMap = "Create a hierarchical study mind map from the supplied document. Return ONLY valid JSON with this shape: {\"title\":\"...\",\"children\":[{\"label\":\"...\",\"description\":\"...\",\"children\":[]}]} . Keep the tree focused on the most important concepts and do not invent facts.";

    public const string Flashcards = "Create 8 to 15 useful study flashcards from the supplied document. Return ONLY valid JSON with this shape: {\"cards\":[{\"question\":\"...\",\"answer\":\"...\",\"explanation\":\"...\"}]}. Every answer must be supported by the document.";

    public const string Quiz = "Create a multiple-choice quiz from the supplied document. Return ONLY valid JSON with this shape: {\"title\":\"...\",\"questions\":[{\"content\":\"...\",\"explanation\":\"...\",\"options\":[{\"text\":\"...\",\"isCorrect\":true}]}]}. Create exactly four options per question, with exactly one correct option. Use 5 to 10 questions.";

    public const string Chat = "Answer the user's question using only the supplied document context. If the answer is not present or cannot be inferred safely from the context, say explicitly that the document does not provide enough information. Do not use outside knowledge. Answer in Vietnamese.";
}
