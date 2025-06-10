export interface LinkT {
    name: string,
    href: string,
    text: string
};

export const links: LinkT[] = [
    {
        name: 'signin',
        href: '/pages/signin',
        text: 'SIGN IN'
    },
    {
        name: 'register',
        href: '/pages/signin',
        text: 'REGISTER'
    },
    {
        name: 'about',
        href: '/pages/about',
        text: 'ABOUT'
    },
    {
        name: 'contact',
        href: '/pages/contact',
        text: 'CONTACT US'
    }
];