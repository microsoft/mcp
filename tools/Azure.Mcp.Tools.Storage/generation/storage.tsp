import "@typespec/http";

using TypeSpec.Http;

/** A bounded projection of the Blob Storage create-container operation for the MCP pilot. */
@service(#{ title: "Storage MCP pilot" })
@server("{endpoint}", "The Storage account endpoint, resolved by the existing MCP account and tenant context.", {
  endpoint: url,
})
namespace StoragePilot;

interface Containers {
  /** Creates a private blob container in a Storage account. */
  @put
  @route("/{container}")
  create(
    @doc("The name of the container to access within the storage account.")
    @path container: string,
    @query restype: "container",
  ): {
    @statusCode statusCode: 201;
  };

  /** Retrieves a container. The existing MCP get/list composition remains handwritten. */
  @get
  @route("/{container}")
  get(@path container: string, @query restype: "container"): void;
}
