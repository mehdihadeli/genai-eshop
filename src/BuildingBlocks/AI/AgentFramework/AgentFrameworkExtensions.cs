using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;

#pragma warning disable SKEXP0110

namespace BuildingBlocks.AI.AgentFramework;

// Using KernelFunctionFactory, KernelPluginFactory and AgentKernelFunctionFactory to create kernel functions and tools.

/// <summary>
/// Provides extension methods for AgentFramework to enhance agent and plugin functionality.
/// These extensions facilitate integration between different AI components and enable seamless
/// function calling between agents and tools.
/// </summary>
public static class AgentFrameworkExtensions
{
    // /// <summary>
    // /// Creates a KernelPlugin from a registered A2A (Agent-to-Agent) agent, enabling
    // /// the agent to be called as a function by other agents or kernel components.
    // /// </summary>
    // /// <param name="kernel">The Semantic Kernel instance to extend</param>
    // /// <param name="agentName">The name of the registered A2AAgent to create a plugin for</param>
    // /// <returns>
    // /// A KernelPlugin that exposes the A2A agent as a callable function
    // /// </returns>
    // /// <exception cref="ArgumentException">
    // /// Thrown when <paramref name="agentName"/> is null, empty, or whitespace
    // /// </exception>
    // /// <exception cref="InvalidOperationException">
    // /// Thrown when no A2AAgent is registered with the specified <paramref name="agentName"/>
    // /// </exception>
    // /// <remarks>
    // /// This method facilitates cross-microservice agent communication by wrapping
    // /// remote A2A agents as local plugin functions. The generated plugin name follows
    // /// the pattern: "{agentName}A2APlugin".
    // /// </remarks>
    // public static KernelPlugin CreatePluginFromA2AAgent(this Kernel kernel, string agentName)
    // {
    //     ArgumentException.ThrowIfNullOrWhiteSpace(agentName);
    //
    //     var agent = kernel.Services.GetRequiredKeyedService<A2AAgent>(agentName);
    //
    //     return KernelPluginFactory.CreateFromFunctions(
    //         $"{agentName}A2APlugin",
    //         [AgentKernelFunctionFactory.CreateFromAgent(agent)]
    //     );
    // }

    // ref: https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/agent-as-function-tool?pivots=programming-language-csharp#create-and-use-an-agent-as-a-function-tool
    /// <summary>
    /// Maps a locally registered agent (such as ChatCompletionAgent) to a callable function tool
    /// using the Microsoft.Extensions.AI abstractions. This enables the agent's capabilities to be
    /// exposed as a tool and invoked by other agents or orchestration logic within the same application or service.
    /// </summary>
    /// <param name="agent">The <see cref="AIAgent"/> instance to expose as a tool function.</param>
    /// <param name="description">An optional description for the generated tool function, used for discovery and documentation.</param>
    /// <returns>
    /// An <see cref="AIFunction"/> representing the local agent, registered as a function tool.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="agent"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This extension enables hierarchical agent composition and function-calling patterns,
    /// where specialized agents can be surfaced as reusable tools for orchestration agents to call.
    /// It standardizes agent integration using Microsoft.Extensions.AI tool conventions and naming patterns,
    /// decoupling your architecture from Semantic Kernel dependencies. The generated function tool name
    /// follows the pattern "{agent.Name}LocalTool".
    /// </remarks>
    public static AIFunction CreateFunctionToolFromLocalAgent(this AIAgent agent, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(agent);

        return agent.AsAIFunction(
            options: new AIFunctionFactoryOptions { Name = $"{agent.Name}LocalAgentTool", Description = description }
        );
    }

    public static AIFunction CreateFunctionToolFromA2AAgent(this AIAgent agent, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(agent);

        return agent.AsAIFunction(
            options: new AIFunctionFactoryOptions { Name = $"{agent.Name}A2AAgentTool", Description = description }
        );
    }

    // https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/function-tools?pivots=programming-language-csharp
    public static AIFunction CreateFunctionToolFromMethod(Delegate function, string name, string? description = null)
    {
        return AIFunctionFactory.Create(
            function
        //new AIFunctionFactoryOptions { Name = name, Description = description }
        );
    }

    // ref: https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/agent-as-mcp-tool
    /// <summary>
    /// Creates an MCP server tool from a locally registered agent using Microsoft.Extensions.AI abstractions,
    /// allowing the agent's capabilities to be exposed as a standardized tool in a Model Context Protocol (MCP) server environment.
    /// </summary>
    /// <param name="agent">The <see cref="AIAgent"/> instance to expose as an MCP server tool.</param>
    /// <param name="description">An optional description for the tool, useful for tool discovery and documentation.</param>
    /// <returns>
    /// A <see cref="McpServerTool"/> representing the local agent as a callable MCP tool.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="agent"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This extension enables seamless registration of local agents as MCP-compatible tools,
    /// supporting modular and composable AI architectures. The resulting tool name follows the "{agent.Name}McpTool" pattern.
    /// </remarks>
    public static McpServerTool CreateMcpToolFromLocalAgent(this AIAgent agent, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(agent);

        return McpServerTool.Create(
            agent.AsAIFunction(
                options: new AIFunctionFactoryOptions { Name = $"{agent.Name}McpTool", Description = description }
            )
        );
    }

    // ref: https://learn.microsoft.com/en-us/agent-framework/user-guide/model-context-protocol/using-mcp-tools?pivots=programming-language-csharp
    public static async Task<IEnumerable<AITool>> CreateFunctionToolsFromMcpTools(this McpClient mcpClient)
    {
        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        return tools.Cast<AITool>();
    }

    public static McpClient GetMcpClientByName(this IServiceProvider serviceProvider, string mcpClientName)
    {
        return serviceProvider.GetRequiredKeyedService<McpClient>(mcpClientName);
    }

    /// <summary>
    /// Registers all functions from the given kernel plugins as tools on an MCP server builder.
    /// </summary>
    /// <param name="builder">The MCP server builder used to register the tools.</param>
    /// <param name="plugins">The collection of kernel plugins whose functions will be exposed as MCP tools.</param>
    /// <returns>
    /// The same <see cref="IMcpServerBuilder"/> instance to enable fluent configuration.
    /// </returns>
    /// <remarks>
    /// Iterates through each plugin and its functions, creating an <see cref="McpServerTool"/>
    /// for each function and registering it with the service collection as a singleton.
    /// </remarks>
    // https://devblogs.microsoft.com/semantic-kernel/building-a-model-context-protocol-server-with-semantic-kernel/
    public static IMcpServerBuilder AddPluginsToMcpServerTools(
        this IMcpServerBuilder builder,
        KernelPluginCollection plugins
    )
    {
        foreach (KernelPlugin plugin in plugins)
        {
            foreach (KernelFunction function in plugin)
            {
                builder.Services.AddSingleton(sp => McpServerTool.Create(function));
            }
        }

        return builder;
    }

    public static IMcpServerBuilder AddPluginToMcpServerTools(this IMcpServerBuilder builder, KernelPlugin plugin)
    {
        foreach (KernelFunction function in plugin)
        {
            builder.Services.AddSingleton(sp => McpServerTool.Create(function));
        }

        return builder;
    }
}
