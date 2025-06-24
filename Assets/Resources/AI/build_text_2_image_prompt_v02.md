# Function Index: build_text_2_image_prompt

## Name
build_text_2_image_prompt

## Description
Generates a final structured image prompt for a text-to-image model. Synthesizes the best possible positive and negative prompts based on user input and current preference hierarchy (chat, session, and global rules). Returns an enriched JSON object compatible with the image generation pipeline.

## Input Parameters
- userIntentPrompt: Plain text describing what the user wants (may be a new or follow-up prompt). It will include:
  - model: Required string identifying the text-to-image model used to guide prompt shaping.
  - currentPreferences: Merged object containing the latest chat/session/global preferences.
  - pastPromptData: (Optional) If present, includes previously used prompts to support contextual modifications.

## Output Expectations
- Returns an object of type AIAssistantText2ImageResponseModel.
- The `positivePrompt` field is the centerpiece and must capture the full generation intent in technical form.
- `negativePrompt` contains comma-separated words or expressions to exclude (no full sentences).
- Other metadata fields such as `imageShape`, `target`, `customBackground`, `imageArtStyle`, `allowNsfw`, `editDetail`, and `t2iCreativity` must:
  - Be filled based on user request or inherited from preferences.
  - Always be reflected and harmonized with the `positivePrompt`.

## Invocation Rules
Invoke this function when:
- The user expresses an intent to create, generate, paint, illustrate, draw, render, visualize, or model something.
- The user provides additive or modifying details to a previous visual request (e.g., "make it brighter", "change it to metallic").
- The user refers to producing a 3D model or scene based on visual assets.

Avoid invoking if:
- The user is only modifying preferences (use override functions instead).
- The user is asking about system logic or rules (use explain_rule_system).

## Assistant Response Guidelines
- Always include an `assistantResponse` string.
- The response must be human-friendly and communicate that:
  - The image is being generated.
  - The provided prompt has been tailored to match both the user request and active preferences.
- The assistant must never imply that it created the image itself — only the prompt.

Example:
"Here’s your image: a silver robotic tiger with glowing blue eyes, built using the prompt that matches your request and session settings."

Avoid:
- Repeating the full `positivePrompt` in the `assistantResponse` unless explicitly requested.
- Using abstract or vague language.

## Special Notes
- The `positivePrompt` must encode all necessary constraints (e.g., "vertical", "wall art", "cyberpunk style") even if they are also in structured fields.
- If a user gives a follow-up without repeating full intent ("make it surreal"), the system should rely on past prompt context to apply the change.
- `customBackground` and `imageArtStyle` are usually null unless explicitly changed by the user; however, if changed, they must appear in the positive prompt.
- `target` affects the tone of the `positivePrompt` (e.g., toy design vs. cinematic scene).
- `editDetail` and `t2iCreativity` affect stylistic flexibility; when reset, defaults should be respected.
