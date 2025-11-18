
using Asp.Versioning;
using HierarchyAccountsSystem.BusinessLogic.Contracts;
using HierarchyAccountsSystem.BusinessLogic.DTO;
using Microsoft.AspNetCore.Mvc;

namespace HierarchyAccountsSystem.Api.Controllers;

/// <summary>
/// Provides API endpoints for managing hierarchical accounts, including retrieval, creation, updating, and deletion of accounts.
/// </summary>
/// <remarks>This controller defines endpoints for common hierarhical account management operations. All actions return standard
/// HTTP status codes to indicate success or failure. The controller expects valid input models and may return error
/// responses for invalid data or if requested resources are not found. API versioning is supported via the [ApiVersion]
/// attribute. All endpoints produce standard response types, including 200 OK, 400 Bad Request, and 500 Internal Server
/// Error.</remarks>
[ApiVersion("1")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class HierarchyAccountController : BaseApiController {
  private readonly IHierarhyAccountService _AccountService;
  public HierarchyAccountController(IHierarhyAccountService accountService) {
    this._AccountService = accountService;
  }

  /// <summary>
  /// Retrieves account details for the specified account identifier.
  /// </summary>
  /// <remarks>Returns a 400 Bad Request response if the account ID is invalid or the account cannot be
  /// retrieved. Returns a 500 Internal Server Error response for unexpected errors.</remarks>
  /// <param name="accountId">The unique identifier of the account to retrieve. Must be a valid, existing account ID.</param>
  /// <returns>An <see cref="IActionResult"/> containing the account details if found; otherwise, a response with an appropriate
  /// error status code and message.</returns>
  [HttpGet("account/get")]
  public async Task<IActionResult> GetAccount([FromQuery] Int32 accountId) {
    try {
      return new ObjectResult(await this._AccountService.GetAccountByIdAsync(accountId));
    } catch (InvalidOperationException ex) {
      return this.StatusCode(StatusCodes.Status400BadRequest, ex.Message);
    } catch (Exception ex) {
      return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
    }
  }

  /// <summary>
  /// Retrieves the hierarchical account tree for the specified account.
  /// </summary>
  /// <remarks>Returns a 400 Bad Request response if the account ID is invalid or the operation cannot be
  /// completed. Returns a 500 Internal Server Error response for unexpected errors.</remarks>
  /// <param name="accountId">The unique identifier of the account for which to retrieve the tree. If null, retrieves the tree for the master root
  /// account.</param>
  /// <returns>An <see cref="IActionResult"/> containing the account tree data if successful; otherwise, a status code indicating
  /// the error.</returns>
  [HttpGet("account/gettree")]
  public async Task<IActionResult> GetAccountTree([FromQuery] Int32? accountId) {
    try {
      return new ObjectResult(await this._AccountService.GetAccountTreeAsync(accountId));
    } catch (InvalidOperationException ex) {
      return this.StatusCode(StatusCodes.Status400BadRequest, ex.Message);
    } catch (Exception ex) {
      return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
    }
  }

  /// <summary>
  /// Creates a new account in the hierarchy using the specified account details.
  /// </summary>
  /// <param name="data">An object containing the information required to create the account, including the account name and parent account
  /// identifier. Cannot be null.</param>
  /// <returns>An <see cref="IActionResult"/> that represents the result of the account creation operation. Returns a 200 OK
  /// response with the created account on success, a 400 Bad Request if the operation is invalid, or a 500 Internal
  /// Server Error for unexpected failures.</returns>
  [HttpPost("account/add")]
  public async Task<IActionResult> AddAccountAsync([FromBody] HierarchyAccountCreateRequest data) {
    try {
      return new ObjectResult(await this._AccountService.AddAccountAsync(data.Name, data.ParentId));
    } catch (InvalidOperationException ex) {
      return this.StatusCode(StatusCodes.Status400BadRequest, ex.Message);
    } catch (Exception ex) {
      return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
    }
  }

  /// <summary>
  /// Moves the specified account to a new parent account among defined.
  /// </summary>
  /// <remarks>Returns a 400 Bad Request response if the move operation is invalid, such as when the specified
  /// accounts do not exist or the move is not permitted. Returns a 500 Internal Server Error response for unexpected
  /// errors.</remarks>
  /// <param name="accountId">The unique identifier of the account to be moved. Must refer to an existing account.</param>
  /// <param name="newParentId">The unique identifier of the new parent account. Must refer to a valid account that can accept the specified
  /// account as a child.</param>
  /// <returns>An <see cref="IActionResult"/> indicating the result of the move operation. Returns a 200 OK response with the
  /// updated account information if successful; otherwise, returns an error response with details.</returns>
  [HttpPut("account/move")]
  public async Task<IActionResult> MoveAccountAsync([FromQuery] Int32 accountId, [FromQuery] Int32 newParentId) {
    try {
      return new ObjectResult(await this._AccountService.UpdateAccountAsync(accountId, newParentId));
    } catch (InvalidOperationException ex) {
      return this.StatusCode(StatusCodes.Status400BadRequest, ex.Message);
    } catch (Exception ex) {
      return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
    }
  }

  /// <summary>
  /// Deletes the account identified by the specified account ID.
  /// </summary>
  /// <remarks>This operation is irreversible. Ensure that the account ID provided is correct before invoking
  /// this method.</remarks>
  /// <param name="accountId">The unique identifier of the account to delete. Must correspond to an existing account.</param>
  /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation. Returns 200 OK if the account was
  /// deleted successfully; returns 400 Bad Request if the account cannot be deleted; returns 500 Internal Server Error
  /// for unexpected errors.</returns>
  [HttpDelete("account/delete")]
  public async Task<IActionResult> DeleteAccountAsync([FromQuery] Int32 accountId) {
    try {
      await this._AccountService.RemoveAccountAsync(accountId);
      return this.Ok();
    } catch (InvalidOperationException ex) {
      return this.StatusCode(StatusCodes.Status400BadRequest, ex.Message);
    } catch (Exception ex) {
      return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
    }
  }
}
