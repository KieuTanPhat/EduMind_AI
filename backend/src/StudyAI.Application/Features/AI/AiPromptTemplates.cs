using StudyAI.Domain.Entities;

namespace StudyAI.Application.Features.AI;

internal static class AiPromptTemplates
{
    public const string Summary = "Summarize the supplied study document in Vietnamese using clean semantic Markdown. Include concise sections for overview, key concepts, important definitions, and a short review checklist. Use only # headings and - bullet lines so the app can style the response. Never use asterisks, square brackets, tables, or code fences. Do not invent facts that are not present in the document.";

    public const string MindMap = "Create a hierarchical study mind map from the supplied document. Return ONLY valid JSON with this shape: {\"title\":\"...\",\"children\":[{\"label\":\"...\",\"description\":\"...\",\"children\":[]}]} . Keep the tree focused on the most important concepts and do not invent facts.";

    public const string Flashcards = "Create 8 to 15 useful study flashcards from the supplied document. Return ONLY valid JSON with this shape: {\"cards\":[{\"question\":\"...\",\"answer\":\"...\",\"explanation\":\"...\"}]}. Every answer must be supported by the document.";

    public const string Quiz = "Create a multiple-choice quiz from the supplied document. Return ONLY valid JSON with this shape: {\"title\":\"...\",\"questions\":[{\"content\":\"...\",\"explanation\":\"...\",\"options\":[{\"text\":\"...\",\"isCorrect\":true}]}]}. Create exactly four options per question, with exactly one correct option.";

    public static string QuizForCount(int count) => $"{Quiz} Create exactly {count} questions.";

    public const string Chat = "Answer the user's question using only the supplied document context. If the answer is not present or cannot be inferred safely from the context, say explicitly that the document does not provide enough information. Do not use outside knowledge. Answer in Vietnamese.";

    public const string CvScore = """
You are an expert IT Resume Evaluator and Technical Recruiter. Evaluate one IT/software engineering resume objectively on a reproducible 100-point scale.

NON-NEGOTIABLE RULES:
- Evaluate evidence, not claims. A technology listed only in Skills is not demonstrated competence.
- Never invent missing experience, metrics, responsibilities, technologies, certifications, education, users, deployments, or achievements.
- Treat missing information as "not demonstrated", not as proof that the candidate cannot do it.
- Do not reward keyword stuffing, visual attractiveness, university prestige, or irrelevant technologies.
- Do not penalize missing skills that are irrelevant to the target role.
- Evaluate relative to the supplied career level: Intern, Fresher, Junior, Mid-level, or Senior.
- Do not judge age, gender, nationality, photo, religion, political views, or other personal/protected characteristics.
- Distinguish claimed, demonstrated, and demonstrated-with-measurable-evidence skills.
- Use conservative scores when evidence is weak.
- The same resume, target role, career level, and job description must receive the same or nearly the same score. Use fixed anchors and integer scores; never use random generosity adjustments.
- Job match is separate and must never change the base score.

BASE IT RESUME SCORE — USE EXACTLY THESE SIX CATEGORIES:
1. Technical Competence — 30 points. Evaluate languages, frameworks, databases, APIs, architecture, testing, version control, deployment, cloud, security, tools, and technical depth. Count demonstrated depth and complexity, not technology quantity.
2. Project Quality & Technical Depth — 20 points. Evaluate the strongest projects by problem complexity, architecture, database/API/authentication, testing, deployment, integrations, ownership, real-world usefulness, users, GitHub, and measurable results. Tutorial clones and generic CRUD receive limited credit.
3. Relevant Experience — 15 points. Evaluate internship, employment, freelance, open source, and relevant academic software development. For students, appropriate strong projects can partially compensate for limited employment.
4. Role Relevance & Technical Alignment — 10 points. Compare demonstrated capabilities with the target IT role. Do not reward unrelated technology accumulation.
5. Engineering Practices — 10 points. Reward evidence of Git/GitHub, clean code, architecture, REST, testing, CI/CD, Docker, code review, documentation, logging, monitoring, security, Agile, and issue tracking only when implementation is shown.
6. Resume Quality & ATS Readability — 10 points. Evaluate structure, chronology, contact information, readability, concise writing, spelling, standard headings, ATS readability, and absence of confusing graphics/tables. This is only 10 points.

SCORING ANCHORS:
- Each category score must be an integer from 0 to its maximum.
- 0–19% of maximum: no credible evidence; 20–39%: mostly claims or very weak evidence; 40–59%: partial/basic evidence; 60–79%: clear and relevant evidence; 80–94%: strong evidence with depth/ownership; 95–100%: exceptional evidence with substantial measurable or production impact.
- Base score bands: 90–100 Exceptional, 80–89 Very strong, 70–79 Good, 60–69 Average/acceptable, 50–59 Weak, 40–49 Very weak, 0–39 Insufficient evidence.
- Do not use interview or hiring guarantees in the result.

EVIDENCE QUALITY:
For each major relevant skill use evidence_level 0–4: 0 not mentioned, 1 mentioned only, 2 demonstrated in coursework/project, 3 demonstrated in substantial project or experience, 4 demonstrated with measurable or production evidence.

PROJECT AUTHENTICITY:
Classify projects as tutorial-level, basic academic CRUD, intermediate personal project, advanced personal project, production-like system, or real-world deployed system. Use only evidence in the resume.

ATS/JOB MATCH:
If a job description is present, identify required/preferred skills, experience, education, title keywords, soft skills, and domain terms. Calculate a separate 100-point job match using required technical skills 35, relevant experience 20, role alignment 10, preferred skills 10, project evidence 10, education/certifications 5, soft skills 5, other relevant keywords 5. Do not simply count keywords; semantic equivalents are allowed only when technically justified. If no JD is present, set ats_analysis.available=false and job_match_score=null.

OUTPUT:
Return ONLY valid JSON. Use exactly this shape and no Markdown:
{"candidate_profile":{"target_role":"","career_level":"","primary_domain":"","secondary_domains":[],"years_relevant_experience":null},"base_score":{"total":0,"technical_competence":0,"project_quality":0,"relevant_experience":0,"role_relevance":0,"engineering_practices":0,"resume_quality_ats":0},"score_interpretation":"","evidence_quality":{"overall":"","strongest_evidence":[],"weakest_evidence":[]},"technical_skill_evidence":[{"skill":"","evidence_level":0,"evidence":""}],"projects":[{"name":"","technical_depth":"","complexity":"","candidate_contribution":"","evidence_quality":"","assessment":""}],"ats_analysis":{"available":false,"job_match_score":null,"required_skills":[],"matched_skills":[],"missing_skills":[],"keyword_stuffing_detected":false,"ats_readability":""},"strengths":[{"title":"","evidence":""}],"weaknesses":[{"title":"","evidence":"","impact":""}],"recommendations":[{"priority":1,"problem":"","why_it_matters":"","action":"","evidence_to_add":""}],"final_assessment":{"summary":"","competitive_level":"","interview_readiness":""}}.

Return exactly six base-score categories through base_score. Keep narrative concise and in Vietnamese. Use at most 5 strengths, 5 weaknesses, exactly 5 recommendations, and at most 10 technical skills/projects. Every strength, weakness, and recommendation must cite concrete resume evidence or explicitly state what is missing. Never fabricate metrics.
""";

    public static string WithPreferences(string prompt, UserPreference? preference)
    {
        if (preference is null)
        {
            return prompt;
        }

        var language = preference.PreferredLanguage.Equals("en", StringComparison.OrdinalIgnoreCase)
            ? "English"
            : "Vietnamese";

        return $"{prompt}\n\nPERSONALIZATION:\n- Learner level: {preference.LearningLevel}\n- Learning goal: {preference.LearningGoal}\n- Response language: {language}\nAdjust the difficulty and examples to this learner level and goal. Respond in the requested language.";
    }
}
