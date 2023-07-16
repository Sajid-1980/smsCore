using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class BaseEntity
{
    [Key]
    public int Id { get; set; }

    public bool IsActive { get; set; }
    public bool IsPublished { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
}

public class PublicMenu : BaseEntity
{
    public string DisplayText { get; set; }
    public string Url { get; set; }
    public int SortOrder { get; set; }
    public string Position { get; set; }
    public string MenuGroup { get; set; }
    public int? ParentId { get; set; }
}

public class PublicContentPost : BaseEntity
{
    public string PreviewImage { get; set; }
    public string DetailImage { get; set; }
    public string PostTitle { get; set; }
    public string ShortDescription { get; set; }
    public string PostContent { get; set; }
    public string Slug { get; set; }
    public int PostCategoryId { get; set; }

    public PublicPostCategory PostCategory { get; set; }

}
public class PublicPostCategory : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<PublicContentPost> ContentPosts { get; set; }
}

public class PublicPage:BaseEntity
{
    public string Tags { get; set; }
    public string Description { get; set; }
    public string PageName { get; set; }
    public string PageTitle { get; set; }
    public string PageContent { get; set; }
    public string Slug { get; set; }
}

public class PublicSlider : BaseEntity
{
    public string FirstLine { get; set; }
    public string SecondLine { get; set; }
    public string ThirdLine { get; set; }
    public string ImagePath { get; set; }
    public string SliderGroup { get; set; }
    public string ButtonText { get; set; }
    public string ButtonUrl { get; set; }
}