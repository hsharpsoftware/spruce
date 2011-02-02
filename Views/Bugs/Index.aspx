﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<WorkItemSummary>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" charset="utf-8">
	$(document).ready(function (){
		setupListPage();
	});
</script>

<table id="workitems-table">
	<thead>
		<tr class="midbg">
			<th style="width:5%">ID</th>
			<th style="width:60%">Title</th>
			<th style="width:20%">Assigned to</th>
			<th style="width:15%">Status</th>
		</tr>
	</thead>
	<tbody>
	<%foreach (WorkItemSummary item in Model){ %>
		<tr>
			<td><%=Html.ActionLink(item.Id.ToString(), "View", new { id = item.Id })%></td>
			<td><%=item.Title %></td>
			<td><%=item.AssignedTo %></td>
			<td><%=item.State %></td>
		</tr>
	<%} %>
	</tbody>
</table>
<br style="clear:both" />

<div id="newitem-container">
    <div id="newitem">&gt;&nbsp;<a href="javascript:toggleNewItem()">New bug</a></div>
    <div id="newitem-textbox">
        <input type="text" id="textbox-newitem" style="width:350px;" />
        <input type="button" value="Save" onclick="addBug()" />
        <input type="button" value="Cancel" onclick="toggleNewItem()" />
    </div>
</div>
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="RightContent" runat="server">
<!-- Filters -->
<div class="darkbg">
	<div class="left toggleitem"><a href="javascript:;"><img src="/Tfs/Spruce/Assets/Images/toggle_minus.png" border="0" /></a></div>
	<div class="left headertitle">Filters</div>
	<br style="clear:both"/>
</div>
<div id="accordian-areas" class="padding"><% %>
<a href="#">All</a><br />
<a href="#">Active</a><br />
<a href="#">Resolved</a><br />
<a href="#">Closed</a><br />
<a href="#">Assigned to me</a><br />
<a href="#">Today</a><br />
<a href="#">Yesterday</a><br />
<a href="#">This week</a><br />
</div>
</asp:Content>