<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WorkItem>" %>
<%@ Import Namespace="Microsoft.TeamFoundation.WorkItemTracking.Client" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="itemcontainer">
        <div id="item">
            <h1>#<%=Model.Id%>&nbsp;<%=Model.Title%></h1>
            <h2><%=Model.State%> | Assigned to <%=Model.Fields["Assigned To"].Value%></h2>
            <p>
                <%=Model.Fields["Repro Steps"].Value%>
            </p>
        </div>
        <div id="favouritebar">
            <div id="date"><%=Model.CreatedDate.ToString()%>&nbsp;|&nbsp;Area: <%=Model.AreaPath%></div>
            <div style="float:right;"><a href="javascript:;"><img src="/Assets/Images/star_on.png" /></a></div>
        </div>
        <br style="clear:both"/>
        <div id="actionbar">
            <a href="/Home/Edit/<%=Model.Id %>">Edit</a>&nbsp;|&nbsp;
            <a href="/Home/Resolve/<%=Model.Id %>">Resolve</a>&nbsp;|&nbsp;
            <a href="/Home/Close/<%=Model.Id %>">Close</a>
        </div>
    </div>

</asp:Content>


