Here is the user's mermaid diagram. Write a reminder to the user to install a Mermaid preview extension to be able to render the diagram.
Please write this into docs/architecture.copilotmd WITHOUT additional explanations on the deployment. Explain only the architecture and data flow.
Make changes if these do not fulfill requirements (do not use </br> in strings when generating the diagram):
 ```mermaid\n{{chart}}\n```.
Ask user if the topology is expected, if not, you should directly update the generated diagram with the user's updated instructions.
Please inform the user that here are the supported hosting technologies: {{hostings}}.