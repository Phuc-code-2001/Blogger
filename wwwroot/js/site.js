
const setupResponsive = () => {

    let windowWidth = $(window).width();

    if(windowWidth > 600) {
        $(".icon_box[role='dropdown_navbar']").hide();
        $(".navbar").show();
    }
    else {
        $(".navbar").hide();
        $(".icon_box[role='dropdown_navbar']").show();
    }

}

const setupHeader = () => {
    let headerHeight = $(".header").height();
    $(".main[role='main']").css('margin-top', headerHeight);
}

const setupActiveLink = () => {
    let nav_links = $(".nav_link");
    
    nav_links.each((index, item) => {
        if(item.href == window.location.href) $(item).addClass("active");
        else $(item).removeClass("active");
    });

}

$(document).ready(() => {

    setupActiveLink();

    setupResponsive();
    setupHeader();

    $(window).resize(() => {
        setupResponsive();
        setupHeader();
    });

    // Setup dropdown navbar
    $(".icon_box[role='dropdown_navbar']").click((e) => {
        $(".navbar").slideToggle(500);
    });

    // Setup Blog Content
    let Contents = document.getElementsByClassName("blog_content");
    for (i = 0; i < Contents.length; i++) {
        let target = document.getElementById(Contents[i].name);
        target.innerHTML = Contents[i].value;
    }

    // Setup header opacicy
    let headerHeight = $(".header").height();
    $(document).scroll((e) => {

        let scrollTop = $(document).scrollTop();

        // console.log(scrollTop);

        if(scrollTop > headerHeight) {
            $(".header").fadeOut(200);
        }
        else {
            $(".header").fadeIn(200)
        }

    });

});