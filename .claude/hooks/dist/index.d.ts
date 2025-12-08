interface PreToolUseHookInput {
    tool_name: string;
    tool_input: any;
    transcript_path: string;
    user_message?: string;
}
interface PreToolUseHookOutput {
    decision: "approve" | "block";
    additionalContext?: string;
}
interface PostToolUseHookInput {
    tool_name: string;
    tool_input: any;
    tool_response: string;
    transcript_path: string;
}
interface PostToolUseHookOutput {
    additionalContext?: string;
}
interface UserPromptSubmitHookInput {
    user_message: string;
    transcript_path: string;
}
interface UserPromptSubmitHookOutput {
    additionalContext?: string;
}
declare function preToolUseHook(input: PreToolUseHookInput): Promise<PreToolUseHookOutput>;
declare function postToolUseHook(input: PostToolUseHookInput): Promise<PostToolUseHookOutput>;
declare function userPromptSubmitHook(input: UserPromptSubmitHookInput): Promise<UserPromptSubmitHookOutput>;
declare const _default: {
    preToolUse: {
        matcher: string;
        handler: typeof preToolUseHook;
    };
    postToolUse: {
        matcher: string;
        handler: typeof postToolUseHook;
    };
    userPromptSubmit: {
        handler: typeof userPromptSubmitHook;
    };
};
export default _default;
