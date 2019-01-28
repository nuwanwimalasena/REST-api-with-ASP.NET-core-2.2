using System;
using System.Collections.Generic;
using BankApi.Models;
using BankApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BankApi.Controllers
{
    [Authorize(Roles = "manager, clerk")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountsController(AccountService accountService)
        {
            _accountService = accountService;
        }
        [Authorize(Roles = "manager, clerk")]
        [HttpGet]
        public ActionResult<List<Account>> Get()
        {
            return _accountService.Get();
        }
        [Authorize(Roles = "manager, clerk")]
        [HttpGet("{id:guid}", Name = "GetAccount")]
        public ActionResult<Account> Get(Guid id)
        {
            var account = _accountService.Get(id);

            if (account == null)
            {
                return NotFound();
            }

            return account;
        }
        [Authorize(Roles = "manager, clerk")]
        [HttpPost]
        public ActionResult<Account> Create(Account account)
        {
            _accountService.Create(account);

            return CreatedAtRoute("GetAccount", new { id = account.Id }, account);
        }
        [Authorize(Roles = "manager")]
        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, Account accIn)
        {
            accIn.Id = id;
            var account = _accountService.Get(id);

            if (account == null)
            {
                return NotFound();
            }
            _accountService.Update(id, accIn);

            return NoContent();
        }
        [Authorize(Roles = "manager")]
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var account = _accountService.Get(id);

            if (account == null)
            {
                return NotFound();
            }

            _accountService.Remove(account.Id);

            return NoContent();
        }
    }
}